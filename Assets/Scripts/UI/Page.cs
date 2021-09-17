using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UTB.Core;
using static UTB.EventSystem.InputEvents;

namespace UTB.UI
{
    public class Page : MonoBehaviour
    {
        private LayoutElement m_LayoutElement;

        [SerializeField]
        private Image m_BackgroundImage;
        [SerializeField]
        private Image m_LoadingScreenImage;
        [SerializeField]
        private RawImage m_RawImage;

        private void Awake()
        {
            Debug.Log("Page::Awake"); 
            m_LayoutElement = GetComponent<LayoutElement>();
            Assert.IsNotNull(m_LayoutElement);
        }

        void Start()
        {
            
        }

        public void Resize(float newWidth, float newHeight, DeviceOrientation newOrientation)
        {
            Assert.IsNotNull(m_LayoutElement);

            // We receive this event BEFORE any resizing of the UI takes place. As such, we need to use the 
            // appropriate one from width and height.
            if (newOrientation == DeviceOrientation.LandscapeLeft || newOrientation == DeviceOrientation.LandscapeRight)
            {
                m_LayoutElement.minWidth = Mathf.Max(newWidth, newHeight);
            }
            else if (newOrientation == DeviceOrientation.Portrait || newOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                m_LayoutElement.minWidth = Mathf.Min(newWidth, newHeight);
            }
            else
            {
                m_LayoutElement.minWidth = newWidth;
            }
        }

        public void EnableLoadingScreen(SceneDescription sceneDescription)
        {
            m_RawImage.enabled = false;
            m_LoadingScreenImage.enabled = true;
            m_LoadingScreenImage.sprite = sceneDescription.LoadingScreen;
            m_BackgroundImage.color = sceneDescription.BackgroundColor;
        }

        public void EnableCaptureScreen(RenderTexture texture)
        {
            m_RawImage.enabled = true;
            m_RawImage.texture = texture;
            m_LoadingScreenImage.enabled = false;
        }
    }
}