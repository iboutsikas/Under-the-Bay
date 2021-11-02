using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

public class ColorShiftModifier : DataBasedModifier
{
    private MeshRenderer m_Renderer;
    private Color m_DefaultColor;

    public Color FromColor = Color.cyan;
    public Color ToColor = Color.magenta;

    public ColorShiftModifier()
    {
        DataProperty = BayDataProperties.Temperature;
    }

    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_DefaultColor = m_Renderer.material.color;
    }

    protected override void ApplyDefaultValues(float value)
    {
        m_Renderer.material.color = m_DefaultColor;
    }

    protected override void ApplyModification(float value, float t)
    {
        m_Renderer.material.color = Color.Lerp(FromColor, ToColor, t);
    }
}
