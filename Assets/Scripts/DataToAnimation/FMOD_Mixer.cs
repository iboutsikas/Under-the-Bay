using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UTB.Data;

public class FMOD_Mixer : DataBasedModifier
{
    public float MinMixerValue = 0.0f;
    public float MaxMixerValue = 1.0f;

    [FMODUnity.EventRef]
    public string NarrationAudioEvent = "";
    FMOD.Studio.EventInstance backgroundEvent;

    [Range(0, 1)]
    public float mixer;
    FMOD.Studio.PARAMETER_ID MixerID;


    public FMOD_Mixer()
    {
        DataProperty = BayDataProperties.Oxygen;
        MinPropertyValue = 0.0f;
        NormalRange = new Vector2(0.0f, 1.0f);
        MaxPropertyValue = 10.0f;
        useNormalRange = false;
    }



    // Start is called before the first frame update
    void Start()
    {

        backgroundEvent = FMODUnity.RuntimeManager.CreateInstance(NarrationAudioEvent);
        backgroundEvent.start();

        backgroundEvent.getDescription(out var mixerEventDescription);
        mixerEventDescription.getParameterDescriptionByName("Mixer", out var mixerParameterDescription);
        MixerID = mixerParameterDescription.id;

    }

    private void OnDestroy()
    {
        backgroundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    protected override void ApplyDefaultValues(float value)
    {
        // We only set mixer here so we can view it in the inspector
        mixer = 0;
        backgroundEvent.setParameterByID(MixerID, mixer);
    }

    protected override void ApplyModification(float value, float t)
    {
        mixer = Mathf.Lerp(MaxMixerValue, MinMixerValue, t);
        backgroundEvent.setParameterByID(MixerID, mixer);
    }
}
