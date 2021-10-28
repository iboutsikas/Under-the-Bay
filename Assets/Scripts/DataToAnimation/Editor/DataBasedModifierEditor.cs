using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


[CustomEditor(typeof(DataBasedModifier), true)]
public class DataBasedModifierEditor : Editor
{
    private List<string> drawnProperties;
    private List<SerializedProperty> debugProperties;
    private SerializedProperty dataProperty;

    private SerializedProperty scriptProperty;
    private SerializedProperty minValue;
    private SerializedProperty maxValue;
    private SerializedProperty ignoreNormalRange;
    private SerializedProperty normalRange;

    private bool genericPropertiesOpen = true;
    private bool modifierPropertiesOpen = true;
    private bool debugPropertiesOpen = true;

    void OnEnable()
    {
        scriptProperty = serializedObject.FindProperty("m_Script");

        dataProperty = serializedObject.FindProperty(nameof(DataBasedModifier.DataProperty));

        minValue = serializedObject.FindProperty(nameof(DataBasedModifier.MinPropertyValue));
        maxValue = serializedObject.FindProperty(nameof(DataBasedModifier.MaxPropertyValue));
        ignoreNormalRange = serializedObject.FindProperty(DataBasedModifier.AdditionalFieldNames.IgnoreNormalRange);
        normalRange = serializedObject.FindProperty(nameof(DataBasedModifier.NormalRange));
        
        drawnProperties = new List<string>()
        {
            scriptProperty.name,
            dataProperty.name,
            minValue.name,
            maxValue.name,
            ignoreNormalRange.name,
            normalRange.name // We always want this one in the list, so it is not drawn by default if we skip it
        };

        debugProperties = new List<SerializedProperty>();

        var it = serializedObject.GetIterator();

        var objectType = serializedObject.targetObject.GetType();

        BindingFlags bindings = BindingFlags.Instance | 
                                BindingFlags.Public | 
                                BindingFlags.NonPublic;

        foreach (var pi in objectType.GetProperties(bindings))
        {
            var attrib = pi.GetCustomAttribute<DebugPropertyAttribute>();

            // This is not a debug property
            if (attrib == null)
                continue;

            if (attrib.Draw)
            {
                var serializedProp = serializedObject.FindProperty(pi.Name);

                if (serializedProp == null)
                {
                    Debug.LogWarning($"Property {pi.Name} is marked as debug property but was not found. If it is private make sure to mark it as [SerializeField]");
                    continue;
                }
                debugProperties.Add(serializedProp);
            }
            drawnProperties.Add(pi.Name);
        }

        foreach (var fi in objectType.GetFields(bindings))
        {
            var attrib = fi.GetCustomAttribute<DebugPropertyAttribute>();

            // This is not a debug property
            if (attrib == null)
                continue;

            if (attrib.Draw)
            {
                var serializedProp = serializedObject.FindProperty(fi.Name);

                if (serializedProp == null)
                {
                    Debug.LogWarning($"Field {fi.Name} is marked as debug property but was not found. If it is private make sure to mark it as [SerializeField]");
                    continue;
                }
                debugProperties.Add(serializedProp);
            }
            drawnProperties.Add(fi.Name);
            
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(scriptProperty, true, Array.Empty<GUILayoutOption>());
        GUI.enabled = true;


        genericPropertiesOpen =
            EditorGUILayout.Foldout(genericPropertiesOpen, "General Properties", EditorStyles.foldoutHeader);

        if (genericPropertiesOpen)
        {
            EditorGUILayout.PropertyField(dataProperty);
            EditorGUILayout.PropertyField(minValue);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Normal Range");
                EditorGUILayout.PropertyField(ignoreNormalRange, GUIContent.none);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            if (ignoreNormalRange.boolValue)
            {
                EditorGUILayout.PropertyField(normalRange, GUIContent.none);
            }

            EditorGUILayout.PropertyField(maxValue);
        }

        modifierPropertiesOpen = EditorGUILayout.Foldout(modifierPropertiesOpen,
            "Modifier Specific Properties",
            EditorStyles.foldoutHeader
        );

        if (modifierPropertiesOpen)
            DrawPropertiesExcluding(serializedObject, drawnProperties.ToArray());

        debugPropertiesOpen = EditorGUILayout.Foldout(debugPropertiesOpen, "Debug", EditorStyles.foldoutHeader);

        if (debugPropertiesOpen)
        {
            foreach (var prop in debugProperties)
            {
                EditorGUILayout.PropertyField(prop);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
