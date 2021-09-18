using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UTB.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

using static UTB.EventSystem.DataEvents;

namespace UTB.Data
{
    public class DataContainer : ScriptableObjectSingleton<DataContainer>
    {
#if UNITY_EDITOR
        // We do not need these outside the editor. During runtime we should be just grabbing the
        // values from the state manager
        private DateTimeOffset? m_FromDate;
        private DateTimeOffset? m_ToDate;
#endif


        [SerializeReference]
        private StationService m_StationService;

        private TimeSpan m_UpdateFrequency;
        private Timer m_Timer;
        [NonSerialized]
        private List<Station> m_Stations;
        [NonSerialized]
        private List<BayData> m_Samples;
        [NonSerialized]
        private Station m_CurrentStation;

        public float UpdateInterval = 15.0f;

        public Station CurrentStation {
            get { return m_CurrentStation; } 
            private set {
                m_CurrentStation = value; 
            }
        }

        public List<Station> Stations
        {
            get { return m_Stations; }
        }

        public BayData CurrentSample { get; private set; } = null;

        public List<BayData> Samples
        {
            get { return m_Samples; }
        }

        public DataContainer()
        {
            m_StationService = new StationService();
            m_Stations = m_Stations ?? new List<Station>();
            m_Samples = m_Samples ?? new List<BayData>();

            m_UpdateFrequency = TimeSpan.FromSeconds(UpdateInterval);
            m_Timer = new Timer(5 * 60 * 1000);

            m_Timer.Elapsed += new ElapsedEventHandler(On_TimerEllapsed);
            m_Timer.Start();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        public static void OnLoad() => Initialize();

        protected override void OnEnable()
        {
            base.OnEnable();

            RequestLoadStationsEvent.Subscribe(On_LoadStationsRequested);
            SelectActiveStationEvent.Subscribe(On_SelectActiveStation);

            RequestLoadSamplesEvent.Subscribe(On_LoadSamplesRequested);
            SelectActiveSampleEvent.Subscribe(On_SelectActiveSample);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);
            }
#endif

            RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
            evt.Fire();
        }

        private void OnDisable()
        {
            RequestLoadStationsEvent.Unsubscribe(On_LoadStationsRequested);
            SelectActiveStationEvent.Unsubscribe(On_SelectActiveStation);

            RequestLoadSamplesEvent.Unsubscribe(On_LoadSamplesRequested);
            SelectActiveSampleEvent.Unsubscribe(On_SelectActiveSample);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SampleDatesChangedEvent.Unsubscribe(On_SampleDatesChanged);
            }
#endif

        }

        private void On_LoadStationsRequested(RequestLoadStationsEvent info)
        {
            try
            {
                m_Stations = m_StationService.GetAllStations();
                m_CurrentStation = m_Stations[0];

                // This way we only fire the event if everything went ok
                StationsLoadedEvent evt = new StationsLoadedEvent();
                evt.Fire();
            }
            catch(Exception e)
            {
                Debug.Log($"[DataContainer] Error while trying to fetch stations: {e.Message}");
            }
        }

        private void On_SelectActiveStation(SelectActiveStationEvent info)
        {
            if (info.Index >= m_Stations.Count)
                return;

            var newStation = m_Stations[info.Index];

            if (newStation.Id == m_CurrentStation?.Id)
                return;

            m_CurrentStation = newStation;

            ActiveStationChangedEvent stationLoadedEvt = new ActiveStationChangedEvent();
            stationLoadedEvt.Index = info.Index;
            stationLoadedEvt.Guid = m_CurrentStation.Id;
            stationLoadedEvt.Fire();

            m_CurrentStation.LastAttempt = DateTimeOffset.Now;

            RequestLoadSamplesEvent samplesLoadEvt = new RequestLoadSamplesEvent();
            samplesLoadEvt.StationId = m_CurrentStation.Id;
            samplesLoadEvt.From = info.From.HasValue ? info.From.Value : m_CurrentStation.LastUpdate;
            samplesLoadEvt.To = info.To.HasValue ? info.To.Value : DateTimeOffset.Now;
            samplesLoadEvt.Fire();          
        }

        private void On_LoadSamplesRequested(RequestLoadSamplesEvent info)
        {
            var requestHasId = info.StationId != Guid.Empty;
            if (requestHasId && (m_CurrentStation == null || m_CurrentStation.Id == Guid.Empty))
                return;
#if UNITY_EDITOR
            if (requestHasId && ( m_CurrentStation.Id != info.StationId))
            {
                Debug.LogWarning($"Tried to load samples for {info.StationId} but the current station is {m_CurrentStation.Id}");
            }
#endif
            var stationId = requestHasId ? info.StationId : m_CurrentStation.Id;

            try
            {
                var from = info.From.HasValue ? info.From.Value : m_CurrentStation.LastUpdate;
                var to = info.To.HasValue ? info.To.Value : DateTimeOffset.Now;

                var samples = m_StationService.GetSamples(stationId, from, to);

                if (samples == null)
                    return;

                m_Samples = samples;
                CurrentSample = m_Samples[0];

                SamplesLoadedEvent evt = new SamplesLoadedEvent();
                evt.Fire();
            }
            catch (AggregateException exp)
            {
                Debug.Log($"[DataContainer] Error while refreshing samples for new station: {exp.Message}\nInner error: {exp.InnerException.Message}");
            }
        }

        private void On_SelectActiveSample(SelectActiveSampleEvent info)
        {
            if (m_Samples == null || m_Samples.Count == 0 || info.SampleIndex >= m_Samples.Count)
                return;

            CurrentSample = m_Samples[info.SampleIndex];
        }

#if UNITY_EDITOR
        private void On_SampleDatesChanged(SampleDatesChangedEvent info)
        {
            if (info.From.HasValue)
                m_FromDate = info.From;

            if (info.To.HasValue)
                m_ToDate = info.To;
        }
#endif

        private void On_TimerEllapsed(object sender, ElapsedEventArgs e)
        {
            if (m_CurrentStation == null)
                return;

            var diff = DateTimeOffset.Now - m_CurrentStation.LastUpdate;
            var diff2 = DateTimeOffset.Now - m_CurrentStation.LastAttempt;

            if (diff <= m_UpdateFrequency && diff2 <= m_UpdateFrequency)
                return;

            m_CurrentStation.LastAttempt = DateTimeOffset.Now;

            DateTimeOffset? from;
            DateTimeOffset? to;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                from = StateManager.Instance.FromDate.HasValue ? StateManager.Instance.FromDate : m_CurrentStation.LastUpdate;
                to = StateManager.Instance.ToDate.HasValue ? StateManager.Instance.ToDate : DateTimeOffset.Now;
            }
            else
            {
                from = m_FromDate.HasValue ? m_FromDate : m_CurrentStation.LastUpdate;
                to = m_ToDate.HasValue ? m_ToDate : DateTimeOffset.Now;
            }
#else
            from = StateManager.Instance.FromDate.HasValue ? StateManager.Instance.FromDate : m_CurrentStation.LastUpdate;
            to = StateManager.Instance.ToDate.HasValue ? StateManager.Instance.ToDate : DateTimeOffset.Now;
#endif

            RequestLoadSamplesEvent evt = new RequestLoadSamplesEvent();
            evt.StationId = m_CurrentStation.Id;
            evt.From = from;
            evt.To = to;

            evt.Fire();
        }
    }
}
