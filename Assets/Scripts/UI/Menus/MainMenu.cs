using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UTB.Core;
using static UTB.EventSystem.SceneLoadingEvents;

namespace UTB.UI
{
    public class MainMenu : MenuPanel
    {
        public SceneConfiguration SceneConfig;
        public List<Button> SceneChangingButtons;
        public Button SystemButton;

        public MainMenu()
            :base(MenuType.MAIN_MENU)
        {

        }

        protected override void Awake()
        {
            base.Awake();

            // We start from 1 as the first is the landing page, and that is not in the menu
            int i = 0;
            foreach (var button in SceneChangingButtons)
            {
                int index = i;
                
                if (index >= SceneConfig.ARScenes.Count)
                    continue;
                
                SceneDescription desc = SceneConfig.ARScenes[index];

                button.onClick.AddListener(() =>
                {
                    RequestSceneLoadEvent evt = new RequestSceneLoadEvent();
                    evt.SceneIndex = desc.BuildIndex;
                    evt.IsAdditive = true;
                    evt.Fire();
                });
                ++i;
            }

            SystemButton.onClick.AddListener(On_SystemButtonClicked);

            SceneLoadedEvent.Subscribe(On_SceneLoaded);

            //Debug.Log("[Main Menu] Awake");
        }

        private void OnDestroy()
        {
            SceneLoadedEvent.Unsubscribe(On_SceneLoaded);
        }

        private void On_SceneLoaded(SceneLoadedEvent info)
        {
            if (IsOpen())
                m_MenuManager.RequestClose(this);
        }

        private void On_SystemButtonClicked()
        {
            this.m_MenuManager.RequestOpen(MenuType.SYSTEM);
        }
    }
}
