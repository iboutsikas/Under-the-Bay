using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

public class ParticleCountModifier : DataBasedModifier
{
    private ParticleSystem m_ParticleSystem;
    private int m_DefaultCount;

    public int MinCount = 1;
    public int MaxCount = 100;

    public ParticleCountModifier()
    {
        DataProperty = BayDataProperties.Salinity;
        MinPropertyValue = 1.0f;
        NormalRange = new Vector2(3.5f, 4.5f);
        MaxPropertyValue = 10;
    }

    public override void Awake() 
    {
        base.Awake();
        m_ParticleSystem = GetComponent<ParticleSystem>();

        if (m_ParticleSystem == null)
        {
            Debug.LogError("This script needs to be attached to a particle system!");
            return;
        }

        m_DefaultCount = m_ParticleSystem.main.maxParticles;
    }

    protected override void ApplyDefaultValues(float value)
    {
        var main = m_ParticleSystem.main;
        main.maxParticles = m_DefaultCount;
    }

    protected override void ApplyModification(float value, float t)
    {
        var main = m_ParticleSystem.main;
        main.maxParticles = (int)Mathf.Lerp(MinCount, MaxCount, t);
    }
}
