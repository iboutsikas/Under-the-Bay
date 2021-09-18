using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ScriptableObjectSingleton<T> : ScriptableObject
    where T : ScriptableObjectSingleton<T>
{
    private static T s_Instance;
    public static T Instance => s_Instance != null ? s_Instance : Initialize();

    protected static T Initialize()
    {
        if (s_Instance != null)
            return s_Instance;

        var instances = Resources.FindObjectsOfTypeAll<T>();
        if (instances.Length > 0)
        {
#if UNITY_EDITOR
            if (instances.Length > 1)
            {
                var type = instances[0].GetType();
                Debug.Log($"[ScriptableObjectSingleton] Found {instances.Length} pre-existing instances for {type.Name}.");
            }
#endif      
            s_Instance = instances[0];
        }
        else
        {
            s_Instance = CreateInstance<T>();
        }

        return s_Instance;
    }

    protected virtual void Awake()
    {
#if UNITY_EDITOR
        Debug.Assert(s_Instance == null);
#endif

        s_Instance = (T)this;

        s_Instance.hideFlags = HideFlags.HideAndDontSave;
    }

    protected virtual void OnDestroy()
    {
        s_Instance = null;
    }

    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += On_PlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void On_PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.playModeStateChanged -= On_PlayModeStateChanged;
            DestroyImmediate(this);
        }
    }
#endif
}

