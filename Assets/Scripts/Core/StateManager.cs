using System;
using UnityEngine;
using static UTB.EventSystem.DataEvents;

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
        }

        private void OnDisable()
        {

        }
    }
}