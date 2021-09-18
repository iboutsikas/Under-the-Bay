using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UTB.Core;
using UTB.Data;
using static UTB.EventSystem.DataEvents;

public class UnderTheBayWindow : EditorWindow
{
    private static string[] StationNames;
    private static int selectedStation = 0;
    private static int selectedSample = 1;
    
    private static DateTimeOffset? fromDate;
    private static string fromDateString;

    private static DateTimeOffset? toDate;
    private static string toDateString;

    [MenuItem("Under the Bay/Data stream")]
    public static void ShowWindow()
    {
        GetWindow<UnderTheBayWindow>(true, "Under the Bay Data stream", false);

        RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
        evt.Fire();
    }

    private void Awake()
    {
        StationsLoadedEvent.Subscribe(On_StationsLoaded);
        SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);
    }

    private void OnDestroy()
    {
        StationsLoadedEvent.Unsubscribe(On_StationsLoaded);
        SampleDatesChangedEvent.Subscribe(On_SampleDatesChanged);
    }



    [InitializeOnLoadMethod]
    private static void OnLoad()
    {
        if (HasOpenInstances<UnderTheBayWindow>())
        {
            var window = GetWindow<UnderTheBayWindow>(true, "Under the Bay Data stream", false);
            StationsLoadedEvent.Subscribe(window.On_StationsLoaded);
            SampleDatesChangedEvent.Subscribe(window.On_SampleDatesChanged);
        }

        RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
        evt.Fire();

        // 8
        DateTimeOffset temp;
        if (DateTimeOffset.TryParse(fromDateString, out temp))
            fromDate = temp;
        if (DateTimeOffset.TryParse(toDateString, out temp))
            toDate = temp;
    }

    private bool DateEditor(ref string dateString, ref DateTimeOffset? date, ref Station station)
    {
        bool dirty = false;
        bool accepted = false;
        EditorGUILayout.BeginHorizontal();
        dateString = EditorGUILayout.TextField(dateString);
        if (GUILayout.Button("Accept"))
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParse(dateString, out temp))
            {
                date = temp;
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
            date = null;
        }
        if (GUILayout.Button("From Station"))
        {
            date = station.LastUpdate;
            dirty = true;
        }
        if (GUILayout.Button("Now"))
        {
            date = DateTimeOffset.Now;
            dirty = true;
        }
        EditorGUILayout.EndHorizontal();

        if (dirty)
            dateString = date.HasValue ? date.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz") : null;

        return accepted;
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
            selectedStation = 0;
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
        fromDateDirty = DateEditor(ref fromDateString, ref fromDate, ref station);

        EditorGUILayout.LabelField("To date:");
        toDateDirty = DateEditor(ref toDateString, ref toDate, ref station);

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

            SelectActiveStationEvent selectEvt = new SelectActiveStationEvent();
            selectEvt.Index = selectedStation;
            selectEvt.Fire();

            RequestLoadSamplesEvent samplesLoadEvt = new RequestLoadSamplesEvent();
            samplesLoadEvt.From = fromDate;
            samplesLoadEvt.To = toDate;
            samplesLoadEvt.Fire();

            return;
        }

        if (serverDropdownDirty)
        {
            SelectActiveStationEvent selectEvt = new SelectActiveStationEvent();
            selectEvt.Index = selectedStation;
            selectEvt.Fire();

            if (fromDate.HasValue || toDate.HasValue)
            {
                RequestLoadSamplesEvent loadEvt = new RequestLoadSamplesEvent();
                loadEvt.From = fromDate;
                loadEvt.To = toDate;
                loadEvt.Fire();
            }
                
        }

        if (fromDateDirty || toDateDirty )
        {
            RequestLoadSamplesEvent loadEvt = new RequestLoadSamplesEvent();
            loadEvt.From = fromDate;
            loadEvt.To = toDate;
            loadEvt.Fire();

            SampleDatesChangedEvent dateEvt = new SampleDatesChangedEvent();


            if (fromDateDirty)
                dateEvt.From = fromDate;

            if (toDateDirty)
                dateEvt.To = toDate;

            dateEvt.Fire();
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
        var names = new List<string>();

        foreach (var station in DataContainer.Instance.Stations)
            names.Add(station.Name);

        StationNames = names.ToArray();
    }

    private void On_SampleDatesChanged(SampleDatesChangedEvent info)
    {
        if (info.From.HasValue)
        {
            fromDate = info.From;
            fromDateString = fromDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
        }

        if (info.To.HasValue)
        {
            toDate = info.To;
            toDateString = toDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzzz");
        }
    }
}
