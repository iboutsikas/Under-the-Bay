using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UTB.Core;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

using static UTB.EventSystem.DataEvents;

namespace UTB.Data
{
    public class DataContainer : ScriptableObjectSingleton<DataContainer>
    {
        [SerializeReference]
        private StationService m_StationService;

        [SerializeField]
        private StationConfiguration m_StationConfig;

        [SerializeField]
        private SystemSettings m_SystemSettings;

        private TimeSpan m_UpdateFrequency;
        private Timer m_Timer;
        [NonSerialized]
        private List<Station> m_Stations;
        [NonSerialized]
        private List<BayData> m_Samples;
        

        public float UpdateInterval = 15.0f;

        [NonSerialized]
        private Station m_CurrentStation;

        public Station CurrentStation {
            get { return m_CurrentStation; } 
            private set {
                Debug.Assert(value != null, "Set CurrentStation to null");
                if (value == null) return;
                
                m_CurrentStation = value;

                ActiveStationChangedEvent evt = new ActiveStationChangedEvent();
                evt.Guid = m_CurrentStation.Id;
                evt.Fire();
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

        public void SelectDefaultStation()
        {
            foreach (var desc in m_StationConfig.Stations)
            {
                if (desc.IsDefaultStation)
                {
                    var temp = m_Stations.Find(s => s.Name == desc.StationName);

                    if (temp != null)
                    {
                        CurrentStation = temp;
                    }
                    break;
                }
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        public static void OnLoad() => Initialize();

        protected override void OnEnable()
        {
            base.OnEnable();

            m_StationConfig = Resources.Load<StationConfiguration>("Station Configuration");
            m_SystemSettings = Resources.Load<SystemSettings>("System Settings");

            RequestLoadStationsEvent.Subscribe(On_LoadStationsRequested);
            SelectActiveStationEvent.Subscribe(On_SelectActiveStation);
            ActiveStationChangedEvent.Subscribe(On_ActiveStationChanged);

            RequestLoadSamplesEvent.Subscribe(On_LoadSamplesRequested);
            SelectActiveSampleEvent.Subscribe(On_SelectActiveSample);

            RequestLoadStationsEvent stationsEvt = new RequestLoadStationsEvent();
            stationsEvt.Fire();

            RequestLoadSamplesEvent samplesEvt = new RequestLoadSamplesEvent();
            samplesEvt.Fire();
        }

        private void OnDisable()
        {
            RequestLoadStationsEvent.Unsubscribe(On_LoadStationsRequested);
            SelectActiveStationEvent.Unsubscribe(On_SelectActiveStation);
            ActiveStationChangedEvent.Unsubscribe(On_ActiveStationChanged);

            RequestLoadSamplesEvent.Unsubscribe(On_LoadSamplesRequested);
            SelectActiveSampleEvent.Unsubscribe(On_SelectActiveSample);
        }


        private void On_LoadStationsRequested(RequestLoadStationsEvent info)
        {
            try
            {
                m_Stations = m_StationService.GetAllStations();
                // This way we only fire the event if everything went ok
                StationsLoadedEvent evt = new StationsLoadedEvent();
                evt.Fire();
            }
            catch(Exception e)
            {
                Debug.Log($"[DataContainer] Error while trying to fetch stations: {e.Message}");
            }

            if (CurrentStation == null)
            {
                SelectDefaultStation();
            }
            else
            {
                Debug.Assert(CurrentStation.Id != Guid.Empty);
                CurrentStation = m_Stations.Find(s => s.Id == m_CurrentStation.Id);
            }
        }

        private void On_SelectActiveStation(SelectActiveStationEvent info)
        { 
            if (info.Guid == Guid.Empty)
            {
                Debug.LogWarning("We tried to select a station with an empty guid");
                return;
            }

            if (info.Guid == CurrentStation.Id)
                return;

            var newStation = m_Stations.FirstOrDefault(s => s.Id == info.Guid);
            
            if (newStation != null)
                CurrentStation = newStation;

            RequestLoadSamplesEvent samplesEvt = new RequestLoadSamplesEvent();
            samplesEvt.StationId = CurrentStation.Id;
            samplesEvt.Fire();
        }

        private void On_ActiveStationChanged(ActiveStationChangedEvent info)
        {
            
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
                DateTimeOffset from;
                if (info.From.HasValue)
                {
                    from = info.From.Value;
                }
                else if (m_SystemSettings.FromDate.HasValue)
                {
                    from = m_SystemSettings.FromDate.Value;
                }
                else
                {
                    from = m_CurrentStation.LastUpdate;
                }

                DateTimeOffset to;
                if (info.To.HasValue)
                {
                    to = info.To.Value;
                }
                else if (m_SystemSettings.ToDate.HasValue)
                {
                    to = m_SystemSettings.ToDate.Value;
                }
                else
                {
                    to = DateTimeOffset.Now.AddMinutes(-0.5);
                }

                var samples = m_StationService.GetSamples(stationId, from, to);

                if (samples == null)
                    return;

                m_Samples = samples;
                CurrentSample = m_Samples.FirstOrDefault();

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

        private void On_TimerEllapsed(object sender, ElapsedEventArgs e)
        {
            if (m_CurrentStation == null)
                return;

            var diff = DateTimeOffset.Now - m_CurrentStation.LastUpdate;
            var diff2 = DateTimeOffset.Now - m_CurrentStation.LastAttempt;

            if (diff <= m_UpdateFrequency && diff2 <= m_UpdateFrequency)
                return;

            m_CurrentStation.LastAttempt = DateTimeOffset.Now;

            RequestLoadSamplesEvent evt = new RequestLoadSamplesEvent();
            evt.StationId = m_CurrentStation.Id;
            evt.Fire();
        }
    }
}
