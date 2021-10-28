using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UTB.Data;

public abstract class DataBasedModifier : MonoBehaviour
{
    protected SystemSettings systemSettings;
    [SerializeField]
    protected bool useNormalRange = true;

#if UNITY_EDITOR
    [DebugProperty]
    public bool EnableOverride = false;
    [DebugProperty]
    public float OverrideValue = 0.0f;
#endif

    public BayDataProperties DataProperty;

    public float MinPropertyValue = 7.1f;
    public Vector2 NormalRange = new Vector2(7.5f, 8.0f);
    public float MaxPropertyValue = 8.5f;

    protected TReturn GetReflectedField<TReturn, TObject, TField>(TObject data, TField field)
    {
        if (data == null)
            return default(TReturn);

        var type = data.GetType();
        string propertyStringName = Enum.GetName(field.GetType(), field);
        FieldInfo info = type.GetField(propertyStringName);

        return (TReturn)info.GetValue(data);
    }

    protected bool WithinRange(float a, float b, float v)
    {
        if (!useNormalRange)
            return false;
        return (v >= a) && (v <= b);
    }

    public virtual void Awake()
    {
        systemSettings = Resources.Load<SystemSettings>("System Settings");
        Debug.Assert(systemSettings != null);
    }

    void FixedUpdate()
    {
        var data = DataContainer.Instance.CurrentSample;

        if (data == null)
            return;

        float value = GetReflectedField<float, BayData, BayDataProperties>(data, DataProperty);

#if UNITY_EDITOR
        if (EnableOverride)
            value = OverrideValue;
#endif
        if (!systemSettings.DataStreamActive
            || value < MinPropertyValue
            || WithinRange(NormalRange.x, NormalRange.y, value)
        )
        {
            ApplyDefaultValues();
            return;
        }

        float t = (value - MinPropertyValue) / (MaxPropertyValue - MinPropertyValue);

        ApplyModification(value, t);
    }

    protected abstract void ApplyDefaultValues();

    protected abstract void ApplyModification(float value, float t);

#if UNITY_EDITOR
    public static class AdditionalFieldNames
    {
        public static string IgnoreNormalRange => nameof(useNormalRange);
    }
#endif
}
