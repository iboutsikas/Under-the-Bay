using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using FMOD.Studio;
using UnityEngine.Serialization;
using UTB.Data;

public class FMODaudioMusic : MonoBehaviour
{
    FMODUnity.EmitterGameEvent PlayEvent = FMODUnity.EmitterGameEvent.None;
    FMODUnity.EmitterGameEvent EndEvent = FMODUnity.EmitterGameEvent.None;

    [FMODUnity.EventRef]
    public string backgroundAudioEvent = "";
    EventInstance backgroundEvent;

    [Header("Pitch Settings")]
    public BayDataProperties PitchProperty = BayDataProperties.Chlorophyll;
    public float PitchPropertyMin = 0;
    public float PitchPropertyMax = 100.0f;
    [FormerlySerializedAs("Pitch")] [Range(0, 1)] public float pitch = 1.0f;
#if UNITY_EDITOR
    public bool OverridePitchProperty = false;
    public float PitchOverride = 0.0f;
#endif
    private PARAMETER_ID PitchID;


    [Header("EQ Settings")]
    public BayDataProperties EQProperty = BayDataProperties.Temperature;
    public float TemperatureMin = 12.0f;
    public float TemperatureMidpoint = 17.0f;
    public float TemperatureMax = 30.0f;
    [Range(0, 400)]
    public float eqhigh;
    PARAMETER_ID EQHighID;
    [Range(1, 20)]
    public float eqlow;
    PARAMETER_ID EQLowID;
#if UNITY_EDITOR
    public bool OverrideEQProperty = false;
    public float EQOverride = 0.0f;
#endif

    [Header("Panning Settings")] public BayDataProperties PanningProperty = BayDataProperties.PH;
    public float PanningPropertyMin = 7.1f;
    public float PanningPropertyMax = 8.5f;
    [Range(0, 1)] public float panning = 0.5f;
    private PARAMETER_ID PanningID;
#if UNITY_EDITOR
    public bool OverridePanningProperty = false;
    public float PanningOverride = 0.0f;
#endif

    [Header("Reverb Settings")]
    public BayDataProperties ReverbProperty = BayDataProperties.Turbidity;
    public float ReverbPropertyMin = 7.1f;
    //public Vector2 ReverbPropertyAvg = new Vector2( 7.0f, 8.0f );
    public float ReverbPropertyMax = 8.5f;
    [Range(0, 100)]
    public float reverb;
    PARAMETER_ID ReverbID;
#if UNITY_EDITOR
    public bool OverrideReverbProperty = false;
    public float ReverbOverride = 0.0f;
#endif


    [Header("Volume Settings")]
    public BayDataProperties VolumeProperty = BayDataProperties.Salinity;
    public float VolumeMin = 4.0f;
    //public Vector2 VolumeAvg = new Vector2(7.3f, 7.7f);
    public float VolumeMax = 10.0f;
    [Range(0, 100)]
    public float volume;
    PARAMETER_ID VolumeID;
#if UNITY_EDITOR
    public bool OverrideVolumeProperty = false;
    public float VolumeOverride = 0.0f;
#endif








    // Start is called before the first frame update
    void Start()
    {
        backgroundEvent = FMODUnity.RuntimeManager.CreateInstance(backgroundAudioEvent);
        backgroundEvent.start();

        backgroundEvent.getDescription(out var eventDescription);

        eventDescription.getParameterDescriptionByName("Pitch", out var pitchParameterDescription);
        PitchID = pitchParameterDescription.id;

        eventDescription.getParameterDescriptionByName("EQHigh", out var eqhighParameterDescription);
        EQHighID = eqhighParameterDescription.id;

        eventDescription.getParameterDescriptionByName("EQLow", out var eqlowParameterDescription);
        EQLowID = eqlowParameterDescription.id;

        eventDescription.getParameterDescriptionByName("Panning", out var panningParameterDescription);
        PanningID= panningParameterDescription.id;

        eventDescription.getParameterDescriptionByName("Reverb", out var reverbParameterDescription);
        ReverbID = reverbParameterDescription.id;

        eventDescription.getParameterDescriptionByName("Volume", out var volumeParameterDescription);
        VolumeID = volumeParameterDescription.id;

    }

    // Update is called once per frame
    void Update()
    {
        backgroundEvent.setParameterByID(PitchID, pitch);
        backgroundEvent.setParameterByID(EQHighID, eqhigh);
        backgroundEvent.setParameterByID(EQLowID, eqlow);
        backgroundEvent.setParameterByID(PanningID, panning);
        backgroundEvent.setParameterByID(ReverbID, reverb);
        backgroundEvent.setParameterByID(VolumeID, volume);

    }

    private void OnDestroy()
    {
        backgroundEvent.stop(STOP_MODE.ALLOWFADEOUT);
    }

    private void FixedUpdate()
    {
        BayData data = DataContainer.Instance.CurrentSample;

        if (data == null)
            return;

        UpdatePitch(data);
        UpdateEQ(data);
        UpdatePanning(data);
        UpdateReverb(data);
        UpdateVolume(data);
    }


    private float GetReflectedFieldValue(BayData data, BayDataProperties field)
    {
        var type = data.GetType();
        string propertyStringName = Enum.GetName(typeof(BayDataProperties), field);
        FieldInfo info = type.GetField(propertyStringName);

        return (float)info.GetValue(data);
    }

    private void UpdatePitch(BayData data)
    {
        float value = GetReflectedFieldValue(data, PitchProperty);

#if UNITY_EDITOR
        if (OverridePitchProperty)
            value = PitchOverride;
#endif  

        pitch = 1.0f - ( (value - PitchPropertyMin) / (PitchPropertyMax - PitchPropertyMin) );
        
    }

    private void UpdateEQ(BayData data)
    {
        float value = GetReflectedFieldValue(data, EQProperty);

#if UNITY_EDITOR
        if (OverrideEQProperty)
            value = EQOverride;
#endif  

        if (value < TemperatureMidpoint)
        {
            eqhigh = 0.0f;
            float t = (value - TemperatureMin) / (TemperatureMidpoint - TemperatureMin);
            // According to the docs we want to be doing 20 -> 1. Not 1 -> 20
            eqlow = Mathf.Lerp(20, 1, t);
        }
        else if (value > TemperatureMidpoint)
        {
            eqlow = 1.0f;
            float t = (value - TemperatureMidpoint) / (TemperatureMax - TemperatureMidpoint);
            eqhigh = Mathf.Lerp(0, 400, t);
        }
        else
        {
            eqlow = 1.0f;
            eqhigh = 0.0f;
        }
    }

    private void UpdatePanning(BayData data)
    {
        float value = GetReflectedFieldValue(data, PanningProperty);

#if UNITY_EDITOR
        if (OverridePanningProperty)
            value = PanningOverride;
#endif  
        panning = (value - PanningPropertyMin) / (PanningPropertyMax - PanningPropertyMin);
        panning = Mathf.Clamp(panning, 0, 1);
    }

    private void UpdateReverb(BayData data)
    {
        float value = GetReflectedFieldValue(data, ReverbProperty);

#if UNITY_EDITOR
        if (OverrideReverbProperty)
            value = ReverbOverride;
#endif  

        float t = (value - ReverbPropertyMin) / (ReverbPropertyMax - ReverbPropertyMin);
        //t = Mathf.Abs(t);
        reverb = Mathf.Lerp(100, 0, t);

    }

    private void UpdateVolume(BayData data)
    {

        float value = GetReflectedFieldValue(data, VolumeProperty);

#if UNITY_EDITOR
        if (OverrideVolumeProperty)
            value = VolumeOverride;
#endif  

        float t = (value - VolumeMin) / (VolumeMax - VolumeMin);
        volume = Mathf.Lerp(0, 100, t);
    }
}

