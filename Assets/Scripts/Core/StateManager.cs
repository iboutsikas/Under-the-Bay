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

            RequestLoadStationsEvent stationsEvt = new RequestLoadStationsEvent();
            stationsEvt.Fire();

            RequestLoadSamplesEvent samplesEvt = new RequestLoadSamplesEvent();
            samplesEvt.Fire();

            StoriesToggledEvent.Subscribe(On_StoriesToggled);
            DataStreamToggledEvent.Subscribe(On_DataToggled);
        }

        private void OnDisable()
        {
            StoriesToggledEvent.Unsubscribe(On_StoriesToggled);
            DataStreamToggledEvent.Unsubscribe(On_DataToggled);
        }

        private void On_StoriesToggled(StoriesToggledEvent info)
        {
            this.SystemSettings.StoriesActive = info.StoriesEnabled;
        }

        private void On_DataToggled(DataStreamToggledEvent info)
        {
            this.SystemSettings.DataStreamActive = info.StreamEnabled;
        }
    }
}