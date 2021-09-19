using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.Core
{
    [Serializable]
    public struct SceneDescription
    {
        public int BuildIndex;
        public string SceneName;
        public Sprite LoadingScreen;
        public Color BackgroundColor;
    }
    
    [Serializable]
    public struct MenuSceneDescription
    {
        public int BuildIndex;
        public string SceneName;
    }

    [CreateAssetMenu(fileName = "Scene Configuration", menuName = "Settings/SceneConfiguration", order = 1)]
    public class SceneConfiguration : ScriptableObject
    {
        public List<SceneDescription> Scenes;
        public List<MenuSceneDescription> MenuScenes;
    }
}