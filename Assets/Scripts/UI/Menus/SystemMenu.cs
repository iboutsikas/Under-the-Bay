using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UTB.EventSystem.StateEvents;

namespace UTB.UI
{
    public class SystemMenu : MenuPanel
    {
        public ToggleSwitch DataSwitch;
        public ToggleSwitch StoriesSwitch;


        protected override void Awake()
        {
            base.Awake();

            Debug.Assert(DataSwitch != null);
            DataSwitch.onToggled.AddListener(On_DataSwitchToggled);
            StoriesSwitch.onToggled.AddListener(On_StoriesSwitchToggled);

            StoriesToggledEvent.Subscribe(On_StoriesToggled);
            DataStreamToggledEvent.Subscribe(On_DataToggled);
        }

        private void OnDestroy()
        {
            StoriesToggledEvent.Unsubscribe(On_StoriesToggled);
            DataStreamToggledEvent.Unsubscribe(On_DataToggled);
        }



        private void On_StoriesSwitchToggled(bool state)
        {
            StoriesToggledEvent evt = new StoriesToggledEvent();
            evt.StoriesEnabled = state;
            evt.Fire();
        }

        private void On_DataSwitchToggled(bool state)
        {
            DataStreamToggledEvent evt = new DataStreamToggledEvent();
            evt.StreamEnabled = state;
            evt.Fire();
        }

        private void On_DataToggled(DataStreamToggledEvent info)
        {
            if (info.StreamEnabled != DataSwitch.Toggled)
                DataSwitch.SetValue(info.StreamEnabled, false);
        }

        private void On_StoriesToggled(StoriesToggledEvent info)
        {
            if (info.StoriesEnabled != StoriesSwitch.Toggled)
                StoriesSwitch.SetValue(info.StoriesEnabled, false);
        }
    }
}
