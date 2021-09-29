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
        private bool m_FadingOut = false;
        private LayoutElement m_LayoutElement;
        private CanvasGroup m_CanvasGroup;

        [SerializeField]
        private float m_FadeDuration = 0.3f;
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
            m_CanvasGroup = GetComponent<CanvasGroup>();
            Assert.IsNotNull(m_CanvasGroup);
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

        public void FadeOut()
        {
            m_FadingOut = true;
            LeanTween.alphaCanvas(m_CanvasGroup, 0.0f, m_FadeDuration)
                .setOnComplete(() =>
                {
                    m_FadingOut = false;
                    this.gameObject.SetActive(false);
                });
        }   

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void Show()
        {
            m_CanvasGroup.alpha = 1.0f;
            this.gameObject.SetActive(true);
            if (m_FadingOut)
            {
                LeanTween.cancel(m_CanvasGroup.gameObject);
                m_FadingOut = false;
            }
        }

        public void EnableLoadingScreen(SceneDescription sceneDescription)
        {
            m_RawImage.enabled = false;
            m_LoadingScreenImage.enabled = true;
            m_LoadingScreenImage.sprite = sceneDescription.LoadingScreen;
            m_BackgroundImage.color = sceneDescription.BackgroundColor;
        }

        public void EnableCaptureScreen(RenderTexture texture, float canvasWidth, float canvasHeight)
        {
            m_RawImage.enabled = true;
            m_RawImage.texture = texture;
            m_RawImage.uvRect = new Rect() {
                x       = -0.01f,
                y       = 0.0f,
                width   = 1.0f,
                height  = 1.0f
            };
            m_LoadingScreenImage.enabled = false;
        }
    }
}