using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

public class ParticleColorTempertatureModifier : DataBasedModifier
{
    private ParticleSystem m_ParticleSystem;
    private Color m_DefaultColor;

    public Color Color1;
    public Color Color2;
    public bool IsWarm;

    public ParticleColorTempertatureModifier()
    {
        DataProperty = BayDataProperties.Temperature;
        useNormalRange = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        if (m_ParticleSystem == null)
        {
            Debug.LogError("This script needs to be attached to a particle system!");
            return;
        }

        m_DefaultColor = m_ParticleSystem.main.startColor.color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var data = DataContainer.Instance.CurrentSample;

        var value = GetReflectedField<float, BayData, BayDataProperties>(data, DataProperty);
#if UNITY_EDITOR
        if (EnableOverride)
            value = OverrideValue;
#endif

        var main = m_ParticleSystem.main;

        if (!systemSettings.DataStreamActive || 
            value < MinPropertyValue || 
            (!IsWarm && value > MaxPropertyValue)
            )
        {
            var startColor = new ParticleSystem.MinMaxGradient(m_DefaultColor);

            main.startColor = startColor;

        }
        else
        {
            float t = (value - MinPropertyValue) / (MaxPropertyValue - MinPropertyValue);
            float h1, s1, v1;
            float h2, s2, v2;

            Color.RGBToHSV(Color1, out h1, out s1, out v1);
            Color.RGBToHSV(Color2, out h2, out s2, out v2);

            var d = h2 - h1;
            var delta = d + ((Mathf.Abs(d) > 0.5) ? ((d < 0) ? 1 : -1) : 0);

            
            float h = (((h1 + (delta * t)) + 1) % 1);
            float s = Mathf.Lerp(s1, s2, t);
            float v = Mathf.Lerp(v1, v2, t);

            var color = Color.HSVToRGB(h, s, v);

            main.startColor = new ParticleSystem.MinMaxGradient(m_DefaultColor, color);
        }
    }

    protected override void ApplyDefaultValues(float value)
    {

        var startColor = new ParticleSystem.MinMaxGradient(m_DefaultColor);

        var main = m_ParticleSystem.main;
        main.startColor = startColor;
    }

    protected override void ApplyModification(float value, float t)
    {
        float h1, s1, v1;
        float h2, s2, v2;

        Color.RGBToHSV(Color1, out h1, out s1, out v1);
        Color.RGBToHSV(Color2, out h2, out s2, out v2);

        var d = h2 - h1;
        var delta = d + ((Mathf.Abs(d) > 0.5) ? ((d < 0) ? 1 : -1) : 0);


        float h = (((h1 + (delta * t)) + 1) % 1);
        float s = Mathf.Lerp(s1, s2, t);
        float v = Mathf.Lerp(v1, v2, t);

        var color = Color.HSVToRGB(h, s, v);
        
        var main = m_ParticleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(m_DefaultColor, color);
    }
}
