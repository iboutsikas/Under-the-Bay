using UnityEngine.SceneManagement;
using static UTB.EventSystem.SceneLoadingEvents;

namespace UTB.Core
{
    public class SceneLoader : MonoBehaviourSingletonPersistent<SceneLoader>
    {
        private int m_CurrentSceneIndex = -1;

        public SceneConfiguration SceneConfig;
        public bool LoadFirstSceneOnStart = true;


        private void OnEnable()
        {
            RequestSceneLoadEvent.Subscribe(On_SceneLoadRequested);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            RequestSceneLoadEvent.Unsubscribe(On_SceneLoadRequested);
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void Start()
        {
            if (!LoadFirstSceneOnStart)
                return;
            var scene = SceneConfig.HomeScreen;
            SceneManager.LoadSceneAsync(scene.BuildIndex, LoadSceneMode.Additive);
        }

        private void On_SceneLoadRequested(RequestSceneLoadEvent info)
        {
            if (m_CurrentSceneIndex != -1)
                SceneManager.UnloadSceneAsync(m_CurrentSceneIndex);

            LoadSceneMode mode = info.IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;

            SceneManager.LoadSceneAsync(info.SceneIndex, mode);
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