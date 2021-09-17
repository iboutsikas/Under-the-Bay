using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static UTB.EventSystem.InputEvents;

namespace UTB.UI
{


    public class Page : MonoBehaviour
    {
        private LayoutElement m_LayoutElement;

        [SerializeField]
        private Image m_Image;
        [SerializeField]
        private RawImage m_RawImage;

        public RectTransform Canvas;


        private void Awake()
        {
            m_LayoutElement = GetComponent<LayoutElement>();
        }

        void Start()
        {

            m_LayoutElement.minWidth = Canvas.rect.width;
        }

        private void OnEnable()
        {
            OrientationChangedEvent.Subscribe(On_OrientationChange);
        }

        private void OnDisable()
        {
            OrientationChangedEvent.Unsubscribe(On_OrientationChange);
        }

        private void On_OrientationChange(OrientationChangedEvent info)
        {
            // NOTE: width and height are swapped AFTER we've gotten this callback
            // So for now we will hack around like that
            m_LayoutElement.minWidth = Canvas.rect.height;
        }

        public void EnableLoadingScreen(Sprite sprite)
        {
            m_RawImage.enabled = false;
            m_Image.enabled = true;
            m_Image.sprite = sprite;
        }

        public void EnableCaptureScreen(RenderTexture texture)
        {
            m_RawImage.enabled = true;
            m_RawImage.texture = texture;
            m_Image.enabled = false;
        }
    }
}