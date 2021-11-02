using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

public class ScaleModifier : DataBasedModifier
{
    private Vector3 m_DefaultScale;

    public Vector3 MinScale;
    public Vector3 MaxScale;

    public ScaleModifier()
    {
        DataProperty = BayDataProperties.Chlorophyll;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_DefaultScale = transform.localScale;
    }

    protected override void ApplyDefaultValues(float value)
    {
        transform.localScale = m_DefaultScale;
    }

    protected override void ApplyModification(float value, float t)
    {
        if (value < NormalRange.x)
        {
            t = (value - MinPropertyValue) / (NormalRange.x - MinPropertyValue);
            transform.localScale = Vector3.Lerp(MinScale, m_DefaultScale, t);
        }
        else
        {
            t = (value - NormalRange.y) / (MaxPropertyValue - NormalRange.y);
            transform.localScale = Vector3.Lerp(m_DefaultScale, MaxScale, t);
        }
    }
}
