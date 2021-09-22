using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UTB.Data;
using static UTB.EventSystem.DataEvents;

namespace UTB.UI
{
    public class HotspotSelector : MonoBehaviour
    {
        public Image TargetImage;
        public StationConfiguration StationConfig;

        // Start is called before the first frame update
        void OnEnable()
        {
            ActiveStationChangedEvent.Subscribe(On_ActiveStationChanged);

            var station = DataContainer.Instance.CurrentStation;
            UpdateMapSprite(station);
        }

        // Update is called once per frame
        void OnDisable()
        {
            ActiveStationChangedEvent.Unsubscribe(On_ActiveStationChanged);
        }

        private void On_ActiveStationChanged(ActiveStationChangedEvent info)
        {
            var station = DataContainer.Instance.CurrentStation;

            UpdateMapSprite(station);
        }
    
        private void UpdateMapSprite(Station station)
        {
            if (station == null)
                return;

            foreach (var desc in StationConfig.Stations)
            {
                if (desc.StationName == station.Name)
                {
                    TargetImage.sprite = desc.SelectedSprite;
                    break;
                }
            }
        }
    }
}
