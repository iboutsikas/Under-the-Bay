using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UTB.Core;
using UTB.Data;
using static UTB.EventSystem.DataEvents;

[Serializable]
public class DatePicker: ISerializationCallbackReceiver
{
    public string DateSerialized;

    public DateTimeOffset? Date;

    public void OnAfterDeserialize()
    {
        if (string.IsNullOrEmpty(DateSerialized))
        {
            Date = null;
        }
        else
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParse(DateSerialized, out temp))
            {
                Date = temp;
            }
            else
            {
                Debug.LogError($"Failed parsing {DateSerialized} during deserialization");
            }
        }
    }

    public void OnBeforeSerialize()
    {
        DateSerialized = Date.HasValue ? Date.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz") : null;
    }

    public bool OnGui(DateTimeOffset resetDate)
    {
        bool dirty = false;
        bool accepted = false;

        EditorGUILayout.BeginHorizontal();
        DateSerialized = EditorGUILayout.TextField(DateSerialized);
        if (GUILayout.Button("Accept"))
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParse(DateSerialized, out temp))
            {
                Date = temp;
                accepted = true;
            }
        }
        if (GUILayout.Button("Reset"))
        {
            dirty = true;
        }
        if (GUILayout.Button("Clear"))
        {
            dirty = true;
            Date = null;
            accepted = true;
        }
        if (GUILayout.Button("From Station"))
        {
            Date = resetDate;
            dirty = true;
        }
        if (GUILayout.Button("Now"))
        {
            Date = DateTimeOffset.Now;
            dirty = true;
        }
        EditorGUILayout.EndHorizontal();

        if (dirty)
            DateSerialized = Date.HasValue ? 
                Date.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz") : 
                null;

        return accepted;
    }
    
    public bool Accept()
    {
        bool accepted = false;

        DateTimeOffset temp;
        if (DateTimeOffset.TryParse(DateSerialized, out temp))
        {
            Date = temp;
            accepted = true;
        }

        return accepted;
    }

    public void Clear()
    {
        DateSerialized = null;
        Date = null;
    }
}

public class UnderTheBayWindow : EditorWindow
{
    [NonSerialized]
    private string[] StationNames;

    private int selectedStation = -1;
    private int selectedSample = 1;

    private DatePicker fromDate = new DatePicker();
    private DatePicker toDate = new DatePicker();

    private Dictionary<int, Guid> m_IndexToGuiMap = new Dictionary<int, Guid>();

    [NonSerialized]
    private StationConfiguration m_StationConfiguration;

    [NonSerialized]
    private SystemSettings m_SystemSettings;

    [MenuItem("Under the Bay/Data stream")]
    public static void ShowWindow()
    {
        GetWindow<UnderTheBayWindow>(true, "Under the Bay Data stream", false);

        RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
        evt.Fire();
    }

    private void OnEnable()
    {
        m_StationConfiguration = Resources.Load<StationConfiguration>("Station Configuration");
        Debug.Assert(m_StationConfiguration != null);
        m_SystemSettings = Resources.Load<SystemSettings>("System Settings");
        Debug.Assert(m_SystemSettings != null);

        if (!fromDate.Date.HasValue)
            fromDate.Date = m_SystemSettings.FromDate;

        if (!toDate.Date.HasValue)
            toDate.Date = m_SystemSettings.ToDate;

        StationsLoadedEvent.Subscribe(On_StationsLoaded);
        SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);

        ActiveStationChangedEvent.Subscribe(On_ActiveStationsChanged);

        if (selectedStation == -1)
        {
            DataContainer.Instance.SelectDefaultStation();
        }
        else
        {
            Guid temp;
            if (m_IndexToGuiMap.TryGetValue(selectedStation, out temp))
            {
                SelectActiveStationEvent selectEvt = new SelectActiveStationEvent();
                selectEvt.Guid = temp;
                selectEvt.Fire();
            }
        }

        RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
        evt.Fire();

        RequestLoadSamplesEvent samplesLoadEvt = new RequestLoadSamplesEvent();
        samplesLoadEvt.Fire();
    }



    private void OnDestroy()
    {
        StationsLoadedEvent.Unsubscribe(On_StationsLoaded);
        SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);

        ActiveStationChangedEvent.Unsubscribe(On_ActiveStationsChanged);
    }

    [InitializeOnLoadMethod]
    private static void OnLoad()
    {
        //if (HasOpenInstances<UnderTheBayWindow>())
        //{
        //}
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Station settings", EditorStyles.boldLabel);

        bool forceUpdate = false;
        bool serverDropdownDirty = false;
        bool fromDateDirty = false;
        bool toDateDirty = false;
        bool sampleDirty = false;

        if (GUILayout.Button("Force refresh data"))
        {
            forceUpdate = true;
            selectedStation = -1;
        }

        if (StationNames == null || StationNames.Length == 0)
        {
            EditorGUILayout.LabelField("No data to show!");
            goto HandleUpdates;
        }


        var newSelection = EditorGUILayout.Popup("Station", selectedStation, StationNames);
        
        if (newSelection != selectedStation)
        {
            selectedStation = newSelection;
            serverDropdownDirty = true;
        }
        #region Station Controls
        var station = DataContainer.Instance.CurrentStation;

        if (station == null)
            goto HandleUpdates;

        EditorGUILayout.LabelField($"Last time the station was updated: {station.LastUpdate}");

        EditorGUILayout.LabelField("From date:");
        fromDateDirty = fromDate.OnGui(station.LastUpdate);

        EditorGUILayout.LabelField("To date:");
        toDateDirty = toDate.OnGui(station.LastUpdate);

        if (GUILayout.Button("Accept both dates"))
        {
            fromDateDirty = fromDate.Accept();
            toDateDirty = toDate.Accept();
        }

        if (GUILayout.Button("Clear both dates"))
        {
            fromDateDirty = toDateDirty = true;
            fromDate.Clear();
            toDate.Clear();
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.Separator();
        #endregion

        #region Sample Settings
        EditorGUILayout.LabelField("Sample settings", EditorStyles.boldLabel);
        var samples = DataContainer.Instance.Samples;
        var sliderLabel = $"Available Samples: {samples.Count}";
        var newSample = selectedSample;

        if (samples.Count > 0)
            sliderLabel += $"\t Selected Sample: {selectedSample}";

        EditorGUILayout.LabelField(sliderLabel);

        if (samples.Count > 0)
            newSample = EditorGUILayout.IntSlider(newSample, 1, samples.Count);

        if (newSample != selectedSample)
        {
            sampleDirty = true;
            selectedSample = newSample;
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.Separator();
        #endregion

        #region Current Sample

        var data = DataContainer.Instance.CurrentSample;
        if (data == null)
            goto HandleUpdates;

        EditorGUILayout.LabelField("Current Sample", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Date sampled: {data.SampleDate}");
        EditorGUILayout.LabelField($"Oxygen Saturation: {data.Oxygen} mg/L");
        EditorGUILayout.LabelField($"Temperature: {data.Temperature} °C");
        EditorGUILayout.LabelField($"Salinity: {data.Salinity} ppt");
        EditorGUILayout.LabelField($"Turbidity: {data.Turbidity} NTU");
        EditorGUILayout.LabelField($"pH: {data.PH}");
        EditorGUILayout.LabelField($"Chlorophyll: {data.Chlorophyll} μg/L");
    #endregion


HandleUpdates:

        if (forceUpdate)
        {
            RequestLoadStationsEvent loadEvt = new RequestLoadStationsEvent();
            loadEvt.Fire();

            RequestLoadSamplesEvent samplesLoadEvt = new RequestLoadSamplesEvent();
            samplesLoadEvt.Fire();

            return;
        }

        if (serverDropdownDirty)
        {
            SelectActiveStationEvent selectEvt = new SelectActiveStationEvent();
            selectEvt.Guid = m_IndexToGuiMap[selectedStation];
            selectEvt.Fire();

            RequestLoadSamplesEvent samplesLoadEvt = new RequestLoadSamplesEvent();
            samplesLoadEvt.Fire();
        }

        if (fromDateDirty || toDateDirty )
        {
            if (fromDateDirty)
                m_SystemSettings.FromDate = fromDate.Date;

            if (toDateDirty)
                m_SystemSettings.ToDate = toDate.Date;

            RequestLoadSamplesEvent loadEvt = new RequestLoadSamplesEvent();
            loadEvt.Fire();
        }

        if (sampleDirty)
        {
            SelectActiveSampleEvent evt = new SelectActiveSampleEvent();
            evt.SampleIndex = selectedSample;
            evt.Fire();
        }
    }


    private void On_StationsLoaded(StationsLoadedEvent info)
    {
        m_IndexToGuiMap.Clear();

        var names = new List<string>();
        int index = 0;

        foreach(var desc in m_StationConfiguration.Stations)
        {
            foreach (var station in DataContainer.Instance.Stations)
            {
                if (station.Name == desc.StationName)
                {
                    names.Add(desc.FriendlyName);
                    m_IndexToGuiMap[index++] = station.Id;
                    break;
                }
            }
        }
        
        StationNames = names.ToArray();
    }

    private void On_SampleDatesChanged(SampleDatesChangedEvent info)
    {
        //if (info.From.HasValue)
        //{
        //    fromDate = info.From;
        //    fromDateString = fromDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
        //}

        //if (info.To.HasValue)
        //{
        //    toDate = info.To;
        //    toDateString = toDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
        //}
    }

    private void On_ActiveStationsChanged(ActiveStationChangedEvent info)
    {
        var pair = m_IndexToGuiMap.First(p => p.Value == info.Guid);
        selectedStation = pair.Key;
    }
}
