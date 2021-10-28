using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DebugPropertyAttribute : PropertyAttribute
{
    public bool Draw;

    public DebugPropertyAttribute(bool draw = true)
    {
        Draw = draw;
    }
}
