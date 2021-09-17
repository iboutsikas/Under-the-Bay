using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.EventSystem
{
    public static class SceneLoadingEvents
    {
        public class RequestSceneLoadEvent : Event<RequestSceneLoadEvent>
        {
            public int SceneIndex;
            public bool IsAdditive = true;
        }

        public class SceneLoadedEvent : Event<SceneLoadedEvent>
        {

        }
    }
}
