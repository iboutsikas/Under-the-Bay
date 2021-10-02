using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "System Settings", menuName = "Settings/System Settings", order = 3)]
public class SystemSettings : ScriptableObject, ISerializationCallbackReceiver
{
    [HideInInspector]
    public string FromDateSerialized;
    [HideInInspector]
    public string ToDateSerialized;

    public DateTimeOffset? FromDate;
    public DateTimeOffset? ToDate;

    public bool StoriesActive;
    public bool DataStreamActive;

    public bool VoiceMixActive;
    public float VoiceMixValue;


    public void OnAfterDeserialize()
    {
        if (string.IsNullOrEmpty(FromDateSerialized))
            FromDate = null;
        else
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParse(FromDateSerialized, out temp))
            {
                FromDate = temp;
            }
            else
            {
                Debug.LogError($"Failed parsing {FromDateSerialized} to FromDate during deserialization");
            }
        }

        if (string.IsNullOrEmpty(ToDateSerialized))
            ToDate = null;
        else
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParse(ToDateSerialized, out temp))
            {
                ToDate = temp;
            }
            else
            {
                Debug.LogError($"Failed parsing {ToDateSerialized} to ToDate during deserialization");
            }
        }
    }

    public void OnBeforeSerialize()
    {
        FromDateSerialized = FromDate.HasValue ? FromDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz") : null;
        ToDateSerialized = ToDate.HasValue ? ToDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz") : null;
    }
}
