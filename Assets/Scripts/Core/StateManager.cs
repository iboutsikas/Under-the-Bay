using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UTB.EventSystem.SceneLoadingEvents;
using static UTB.EventSystem.DataEvents;

namespace UTB.Core
{
    public class StateManager : MonoBehaviourSingletonPersistent<StateManager>
    {
        private int m_CurrentSceneIndex = -1;

        public DateTimeOffset? FromDate;
        public DateTimeOffset? ToDate;

        public override void Awake()
        {
            base.Awake();

        }

        private void OnEnable()
        {
            RequestSceneLoadEvent.Subscribe(On_SceneLoadRequested);
            SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            RequestSceneLoadEvent.Unsubscribe(On_SceneLoadRequested);
            SampleDatesChangedEvent.Unsubscribe(On_SampleDatesChanged);

            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        

        private void Start()
        {
            //SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        }

        private void On_SceneLoadRequested(RequestSceneLoadEvent info)
        {
            if (m_CurrentSceneIndex != -1)
                SceneManager.UnloadSceneAsync(m_CurrentSceneIndex);

            LoadSceneMode mode = info.IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;

            SceneManager.LoadSceneAsync(info.SceneIndex, mode);
        }

        private void On_SampleDatesChanged(SampleDatesChangedEvent info)
        {
            if (info.From.HasValue)
                FromDate = info.From.Value;

            if (info.To.HasValue)
                ToDate = info.To.Value;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_CurrentSceneIndex = scene.buildIndex;

            SceneLoadedEvent evt = new SceneLoadedEvent();
            evt.BuildIndex = m_CurrentSceneIndex;

            evt.Fire();
        }
    }
}