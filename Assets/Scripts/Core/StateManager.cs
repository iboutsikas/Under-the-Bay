using System;
using UnityEngine;
using static UTB.EventSystem.DataEvents;
using static UTB.EventSystem.StateEvents;

namespace UTB.Core
{
    public class StateManager : MonoBehaviourSingletonPersistent<StateManager>
    {
        public SystemSettings SystemSettings;

        public override void Awake()
        {
            base.Awake();

        }

        private void OnEnable()
        {
            SystemSettings.FromDate = null;
            SystemSettings.ToDate = null;
            SystemSettings.DataStreamActive = true;
            

            RequestLoadStationsEvent stationsEvt = new RequestLoadStationsEvent();
            stationsEvt.Fire();

            RequestLoadSamplesEvent samplesEvt = new RequestLoadSamplesEvent();
            samplesEvt.Fire();

            DataStreamToggledEvent streamEvt = new DataStreamToggledEvent()
            {
                StreamEnabled = true
            };
            streamEvt.Fire();

            StoriesToggledEvent.Subscribe(On_StoriesToggled);
            DataStreamToggledEvent.Subscribe(On_DataToggled);
            VoiceMixToggledEvent.Subscribe(On_VoiceMixToggled);
            VoiceMixChangedEvent.Subscribe(On_VoiceMixChanged);
        }        

        private void OnDisable()
        {
            StoriesToggledEvent.Unsubscribe(On_StoriesToggled);
            DataStreamToggledEvent.Unsubscribe(On_DataToggled);
            VoiceMixToggledEvent.Unsubscribe(On_VoiceMixToggled);
            VoiceMixChangedEvent.Unsubscribe(On_VoiceMixChanged);
        }

        private void On_StoriesToggled(StoriesToggledEvent info)
        {
            this.SystemSettings.StoriesActive = info.StoriesEnabled;
        }

        private void On_DataToggled(DataStreamToggledEvent info)
        {
            this.SystemSettings.DataStreamActive = info.StreamEnabled;
        }

        private void On_VoiceMixToggled(VoiceMixToggledEvent info)
        {
            this.SystemSettings.VoiceMixActive = info.MixEnabled;
        }

        private void On_VoiceMixChanged(VoiceMixChangedEvent info)
        {
            this.SystemSettings.VoiceMixValue = info.Value;
        }
    }
}