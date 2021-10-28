using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UTB.Data;

[RequireComponent(typeof(MeshRenderer))]
public class ColorTemperatureModifier : DataBasedModifier
{
    private const float s_1Over360 = 1.0f / 360.0f;
    
    private MeshRenderer m_Renderer;
    private Color m_DefaultColor;
    [Range(0, 360)]
    public int MaxHueToAdd;

#if UNITY_EDITOR
    public Color ComputedColor;
#endif




    public ColorTemperatureModifier()
    {
        DataProperty = BayDataProperties.Temperature;
    }

    private void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_DefaultColor = m_Renderer.material.color;
    }
    
    protected override void ApplyDefaultValues()
    {
        m_Renderer.material.color = m_DefaultColor;
    }

    protected override void ApplyModification(float value, float t)
    {
        Color.RGBToHSV(m_DefaultColor, out float h, out float s, out float v);
        float hueAddition = 0;

        if (value < NormalRange.x)
        {
            // Make it colder            

            t = (value - MinPropertyValue) / (NormalRange.x - MinPropertyValue);
            hueAddition = Mathf.Lerp(0, MaxHueToAdd, t) * s_1Over360;
            h -= hueAddition;
        }
        else
        {
            // Make it warmer

            t = (value - NormalRange.y) / (MaxPropertyValue - NormalRange.y);
            hueAddition = Mathf.Lerp(0, MaxHueToAdd, t) * s_1Over360;
            h += hueAddition;
        }

        // If we went below 0, we need to make it go from 360 down
        if (h < 0.0f)
        {
            h = 1.0f - h;
        }

        // We need to wrap back
        if (h > 1.0f)
        {
            h %= 1;
        }

        s = 0.90f;

        var newColor = Color.HSVToRGB(h, s, v);

#if UNITY_EDITOR
        ComputedColor = newColor;
#endif
        m_Renderer.material.color = newColor;
    }
}
