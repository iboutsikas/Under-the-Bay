using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Data;

public class ParticlePropelModifier : DataBasedModifier
{
    private float m_DefaultModifier;

    private ParticleSystem m_ParticleSystem;

    public float MinModifier = 1.0f;
    public float MaxModifier = 2.0f;

    public ParticlePropelModifier()
    {
        DataProperty = BayDataProperties.PH;
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

        m_DefaultModifier = m_ParticleSystem.main.simulationSpeed;
    }
    
    protected override void ApplyDefaultValues()
    {
        var main = m_ParticleSystem.main;
        main.simulationSpeed = m_DefaultModifier;
    }

    protected override void ApplyModification(float value, float t)
    {
        var main = m_ParticleSystem.main;
        main.simulationSpeed = Mathf.Lerp(MinModifier, MaxModifier, t); 
    }
}
