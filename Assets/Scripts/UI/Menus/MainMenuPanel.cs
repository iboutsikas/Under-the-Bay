using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UTB.Core;
using static UTB.EventSystem.SceneLoadingEvents;

namespace UTB.UI
{
    public class MainMenuPanel : MenuPanel
    {
        public SceneConfiguration SceneConfig;
        public List<Button> SceneChangingButtons;
        public Button SystemButton;

        protected override void Awake()
        {
            base.Awake();

            // We start from 1 as the first is the landing page, and that is not in the menu
            int i = 1;
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

            SceneLoadedEvent.Subscribe(On_SceneLoaded);
        }

        private void OnDestroy()
        {
            SceneLoadedEvent.Unsubscribe(On_SceneLoaded);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void On_SceneLoaded(SceneLoadedEvent info)
        {
            if (IsOpen())
            {
                PopOut();
            }
        }
    }
}
