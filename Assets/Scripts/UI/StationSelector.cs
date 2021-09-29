using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTB.Data;
using static UTB.EventSystem.DataEvents;

namespace UTB.UI
{
    public class StationSelector : MonoBehaviour
    {
        private Dictionary<int, Guid> m_IndexToGuiMap = new Dictionary<int, Guid>();
        
        private TMP_Dropdown m_Dropdown;

        [SerializeField]
        private StationConfiguration m_StationConfiguration;
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

            PopulateDropdown();
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
            var pair = m_IndexToGuiMap.First(p => p.Value == info.Guid);

            m_Dropdown.SetValueWithoutNotify(pair.Key);
        }

        private void On_DropdownValueChanged(int index)
        {
            SelectActiveStationEvent evt = new SelectActiveStationEvent();
            evt.Guid = m_IndexToGuiMap[index];
            evt.Fire();
        }

        private void On_StationsLoaded(StationsLoadedEvent info)
        {
            if (m_Dropdown == null)
                return;

            PopulateDropdown();
        }

        void PopulateDropdown()
        {
            m_Dropdown.options.Clear();
            int index = 0;
            foreach (var desc in m_StationConfiguration.Stations)
            {
                foreach (var station in DataContainer.Instance.Stations)
                {
                    if (station.Name == desc.StationName)
                    {
                        m_Dropdown.options.Add(new TMP_Dropdown.OptionData(desc.FriendlyName));
                        m_IndexToGuiMap[index++] = station.Id;
                        break;
                    }
                }
            }
        }

        private void FillServerInfo(BayData sample)
        {
            if (sample == null)
            {
                ServerInfoText.text = "No sample available";
                return;
            }

            ServerInfoText.text =
                $"Oxygen {sample.Oxygen} mg/L\n" +
                $"Temperature {sample.Temperature:0.#} °C\n" +
                $"Salinity {sample.Salinity} ppt\n" +
                $"Turbidity {sample.Turbidity} NTU\n" +
                $"Chlorophyll {sample.Chlorophyll} μg/L";
        }
    }
}
