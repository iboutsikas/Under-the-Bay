using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

[Flags]
public enum RotationAxis
{
    X       = 1 << 0,
    Y       = 1 << 1,
    Z       = 1 << 2
};

public class TeeteringModifier : DataBasedModifier
{
    private Quaternion m_DefaultRotation;

    public RotationAxis Axis = RotationAxis.X;

    public Vector3 MinDeviation = new Vector3(45, 45, 45);
    public Vector3 MaxDeviation = new Vector3(90, 90, 90);

    public float Speed = 1;

    public TeeteringModifier()
    {
        DataProperty = BayDataProperties.PH;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_DefaultRotation = transform.rotation;
    }

    protected override void ApplyDefaultValues()
    {
        transform.rotation = m_DefaultRotation;
    }

    protected override void ApplyModification(float value, float t)
    {
        Vector3 deviation = Vector3.zero;
        Vector3 euler = m_DefaultRotation.eulerAngles;

        if (value < NormalRange.x)
        {
            t = (value - MinPropertyValue) / (NormalRange.x - MinPropertyValue);
            deviation = Vector3.Lerp(MinDeviation, deviation, t);
        }
        else
        {
            t = (value - NormalRange.y) / (MaxPropertyValue - NormalRange.y);
            deviation = Vector3.Lerp(deviation, MaxDeviation, t);
        }

        var sin = Mathf.Sin(Time.time * Speed);
        deviation *= sin;

        if (Axis.HasFlag(RotationAxis.X))
        {
            euler.x = deviation.x;
        }

        if (Axis.HasFlag(RotationAxis.Y))
        {
            euler.y = deviation.y;
        }

        if (Axis.HasFlag(RotationAxis.Z))
        {
            euler.z = deviation.z;
        }

        transform.rotation = Quaternion.Euler(euler);
    }

}
