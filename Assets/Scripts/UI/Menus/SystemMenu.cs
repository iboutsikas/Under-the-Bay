using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UTB.EventSystem.StateEvents;

namespace UTB.UI
{
    public class SystemMenu : MenuPanel
    {
        public ToggleSwitch DataSwitch;
        public ToggleSwitch StoriesSwitch;
        public Toggle VoiceMixToggle;
        public Slider VoiceMixSlider;
        public Button MapButton;
        public Button InfoButton;

        public SystemMenu()
            :base(MenuType.SYSTEM)
        {

        }

        protected override void Awake()
        {
            base.Awake();

            Debug.Assert(DataSwitch != null);
            DataSwitch.onToggled.AddListener(On_DataSwitchToggled);
            
            Debug.Assert(StoriesSwitch != null);
            StoriesSwitch.onToggled.AddListener(On_StoriesSwitchToggled);

            Debug.Assert(VoiceMixToggle != null);
            VoiceMixToggle.onValueChanged.AddListener(On_VoiceMixToggled);

            Debug.Assert(VoiceMixSlider != null);
            VoiceMixSlider.onValueChanged.AddListener(On_VoiceSliderChanged);

            Debug.Assert(MapButton != null);
            MapButton.onClick.AddListener(On_MapButtonPressed);

            Debug.Assert(InfoButton != null);
            InfoButton.onClick.AddListener(On_InfoButtonPressed);

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

        private void On_VoiceMixToggled(bool state)
        {
            throw new NotImplementedException();
        }

        private void On_VoiceSliderChanged(float arg0)
        {
            throw new NotImplementedException();
        }

        private void On_MapButtonPressed()
        {
            m_MenuManager.RequestOpen(MenuType.MAP);
        }

        private void On_InfoButtonPressed()
        {
            m_MenuManager.RequestOpen(MenuType.ABOUT);
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
