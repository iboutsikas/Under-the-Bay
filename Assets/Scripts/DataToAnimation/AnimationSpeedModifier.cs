using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimationSpeedModifier : DataBasedModifier
{
    private Animator animator;
    private int propertyHash;
    private float defaultSpeed = 1.0f;


    public float minSpeed = 1.0f;
    public float maxSpeed = 5.0f;
    public string parameterName = "UTB_Speed";
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        Debug.Assert(animator != null, 
            $"[Object: {name}] No animator detected. This script can only work when there is an animator attached");

        propertyHash = Animator.StringToHash(parameterName);

        if (HasParameter(animator, propertyHash))
        {
            defaultSpeed = animator.GetFloat(propertyHash);
        }
        else
        {
            Debug.LogWarning($"[Object: {name}] Could not find parameter {parameterName} in the animator");
        }

    }

    protected override void ApplyDefaultValues()
    {
        animator.SetFloat(propertyHash, defaultSpeed);
    }

    protected override void ApplyModification(float value, float t)
    {
        float speed = Mathf.Lerp(minSpeed, maxSpeed, t);
        animator.SetFloat(propertyHash, speed);
    }

    private bool HasParameter(Animator animator, int parameterHash)
    {
        foreach (var param in animator.parameters)
        {
            if (param.nameHash == parameterHash)
                return true;
        }

        return false;

    }
}
