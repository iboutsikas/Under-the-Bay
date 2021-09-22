using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UTB.EventSystem.InputEvents;

namespace UTB.UI
{
    [Serializable]
    public struct ScalableElement
    {
        public LayoutElement LayoutElement;
        public float LandscapeWidth;
        public float PortraitWidth;
    };

    public class HorizontalScaler : MonoBehaviour
    {
        public ScalableElement[] Elements;

        void OnEnable()
        {
            OrientationChangedEvent.Subscribe(On_OrientationChange);
        }        

        private void OnDisable()
        {
            OrientationChangedEvent.Unsubscribe(On_OrientationChange);
        }

        private void On_OrientationChange(OrientationChangedEvent info)
        {
            if (Elements == null || Elements.Length == 0)
                return;

            if (info.NewOrientation == DeviceOrientation.Unknown)
                return;

            var isLandScape = info.NewOrientation == DeviceOrientation.LandscapeLeft
                || info.NewOrientation == DeviceOrientation.LandscapeRight;

            foreach (var e in Elements)
            {
                e.LayoutElement.flexibleWidth = isLandScape ? e.LandscapeWidth : e.PortraitWidth;
            }
        }
    }
}
