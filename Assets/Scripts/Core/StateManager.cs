using System;
using UnityEngine;

using static UTB.EventSystem.DataEvents;

namespace UTB.Core
{
    public class StateManager : MonoBehaviourSingletonPersistent<StateManager>
    {
        public DateTimeOffset? FromDate;
        public DateTimeOffset? ToDate;

        public override void Awake()
        {
            base.Awake();

        }

        private void OnEnable()
        {
            SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);

        }

        private void OnDisable()
        {
            SampleDatesChangedEvent.Unsubscribe(On_SampleDatesChanged);

        }

        private void On_SampleDatesChanged(SampleDatesChangedEvent info)
        {
            if (info.From.HasValue)
                FromDate = info.From.Value;

            if (info.To.HasValue)
                ToDate = info.To.Value;
        }


    }
}