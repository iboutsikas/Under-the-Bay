using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTB.Data;
using static UTB.EventSystem.DataEvents;

namespace UTB.UI
{
    public class StationSelector : MonoBehaviour
    {
        private TMP_Dropdown m_Dropdown;

        public TMP_Text ServerInfoText;

        void Awake()
        {
            m_Dropdown = GetComponent<TMP_Dropdown>();

            m_Dropdown.onValueChanged.AddListener(On_DropdownValueChanged);
        }
        

        private void OnEnable()
        {
            ActiveStationChangedEvent.Subscribe(On_ActiveStationChanged);
            StationsLoadedEvent.Subscribe(On_StationsLoaded);

            RequestLoadStationsEvent evt = new RequestLoadStationsEvent();
            evt.Fire();
        }

        

        private void OnDisable()
        {
            ActiveStationChangedEvent.Unsubscribe(On_ActiveStationChanged);
            StationsLoadedEvent.Unsubscribe(On_StationsLoaded);
        }

        private void FixedUpdate()
        {
            var sample = DataContainer.Instance.CurrentSample;

            FillServerInfo(sample);
        }


        private void On_ActiveStationChanged(ActiveStationChangedEvent info)
        {
            m_Dropdown.value = info.Index;

            var sample = DataContainer.Instance.CurrentSample;

            FillServerInfo(sample);
        }

        private void On_DropdownValueChanged(int index)
        {
            SelectActiveStationEvent evt = new SelectActiveStationEvent();
            evt.Index = index;
            evt.Fire();
        }

        private void On_StationsLoaded(StationsLoadedEvent info)
        {
            if (m_Dropdown == null)
                return;
            var dc = DataContainer.Instance;


            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var station in dc.Stations)
            {
                options.Add(new TMP_Dropdown.OptionData(station.Name));
            }
            m_Dropdown.options = options;

            var sample = dc.CurrentSample;

            FillServerInfo(sample);
        }

        private void FillServerInfo(BayData sample)
        {
            if (sample == null)
                return;

            ServerInfoText.text =
                $"Oxygen {sample.Oxygen} mg/L\n" +
                $"Temperature {sample.Temperature:0.#} °C\n" +
                $"Salinity {sample.Salinity} ppt\n" +
                $"Turbidity {sample.Turbidity} NTU\n" +
                $"Chlorophyll {sample.Chlorophyll} μg/L";
        }
    }
}
