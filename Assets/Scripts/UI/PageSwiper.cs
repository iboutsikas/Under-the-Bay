using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UTB.EventSystem.InputEvents;
using static UTB.EventSystem.SceneLoadingEvents;

[Serializable]
public struct SceneDescription
{
    public Sprite LoadingScreen;
};

namespace UTB.UI
{


    public class PageSwiper : MonoBehaviour
    {
        private RectTransform m_RectTransform;
        private Page[] m_Pages;
        private RenderTexture m_CaptureTexture;
        private int m_CurrentScene = 0;
        private int m_NextScene = 1;

        [Range(0, 1)]
        public float PercentThreshold = 0.8f;
        [Range(0, 1)]
        public float ElasticDragThreshold = 0.05f;

        public RectTransform Canvas;
        public List<SceneDescription> Scenes;


        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_Pages = GetComponentsInChildren<Page>();

            HidePages();
        }

        private void OnEnable()
        {
            SwipeStartEvent.Subscribe(On_SwipeStart);
            SwipeProgressEvent.Subscribe(On_SwipeProgress);
            SwipeEndEvent.Subscribe(On_SwipeEnd);
            OrientationChangedEvent.Subscribe(On_OrientationChange);

            SceneLoadedEvent.Subscribe(On_SceneLoaded);
        }

        private void OnDisable()
        {
            SwipeStartEvent.Unsubscribe(On_SwipeStart);
            SwipeProgressEvent.Unsubscribe(On_SwipeProgress);
            SwipeEndEvent.Unsubscribe(On_SwipeEnd);
            OrientationChangedEvent.Unsubscribe(On_OrientationChange);

            SceneLoadedEvent.Unsubscribe(On_SceneLoaded);

        }

        private void On_OrientationChange(OrientationChangedEvent info)
        {
            var pos = m_RectTransform.anchoredPosition;

            pos.x = -m_NextScene * Canvas.rect.width;
        }

        private void ShowPages()
        {
            foreach (var image in m_Pages)
            {
                image.gameObject.SetActive(true);
            }
        }

        private void HidePages()
        {
            foreach (var image in m_Pages)
            {
                image.gameObject.SetActive(false);
            }
        }

        private void MoveFullLeft()
        {
            var pos = m_RectTransform.anchoredPosition;
            pos.x = -(m_Pages.Length - 1) * Canvas.rect.width;

            m_RectTransform.anchoredPosition = pos;
        }

        private void MoveFullRight()
        {
            m_RectTransform.anchoredPosition = new Vector2(0, 0);
        }

        private void On_SwipeStart(SwipeStartEvent info)
        {
            if (!info.Direction.HasFlag(SwipeDirection.LEFT) && !info.Direction.HasFlag(SwipeDirection.RIGHT))
                return;

            m_CaptureTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);

            ScreenCapture.CaptureScreenshotIntoRenderTexture(m_CaptureTexture);

            if (info.Direction.HasFlag(SwipeDirection.LEFT))
            {
                AdvanceNextScene();
                m_Pages[0].EnableCaptureScreen(m_CaptureTexture);
                m_Pages[1].EnableLoadingScreen(Scenes[m_NextScene].LoadingScreen);

                MoveFullRight();
            }
            else
            {
                RetractNextScene();
                m_Pages[0].EnableLoadingScreen(Scenes[m_NextScene].LoadingScreen);
                m_Pages[1].EnableCaptureScreen(m_CaptureTexture);

                MoveFullLeft();

            }

            ShowPages();
        }

        private void On_SwipeProgress(SwipeProgressEvent evt)
        {
            var pos = m_RectTransform.anchoredPosition;
            pos.x += evt.Delta.x;

            // Negative offset since we start from 0
            var maximumPagesOffset = -(m_Pages.Length - 1);
            var canvasWidth = Canvas.rect.width;
            var elasticMargin = ElasticDragThreshold * canvasWidth;
            var negativeOffset = maximumPagesOffset * canvasWidth;

            if (pos.x <= negativeOffset - elasticMargin)
                pos.x = negativeOffset - elasticMargin;

            if (pos.x > elasticMargin)
                pos.x = elasticMargin;

            m_RectTransform.anchoredPosition = pos;
        }

        private void On_SwipeEnd(SwipeEndEvent info)
        {
            if (!info.Direction.HasFlag(SwipeDirection.LEFT) && !info.Direction.HasFlag(SwipeDirection.RIGHT))
                return;

            var pos = m_RectTransform.anchoredPosition;
            var canvasWidth = Canvas.rect.width;

            float difference = (info.StartPosition - info.Position).x;

            // Takes a value from -1, 0 basically, so we need to clamp it next
            int screenOffsetCount = (int)(pos.x / canvasWidth);

            if (screenOffsetCount <= -m_Pages.Length)
            {
                screenOffsetCount = -(m_Pages.Length - 1);
            }

            if (screenOffsetCount > 0)
            {
                screenOffsetCount = 0;
            }

            float pctMoved = Mathf.Abs(difference / canvasWidth);

            bool swappedLoadingScreen = false;

            if (info.Direction.HasFlag(SwipeDirection.LEFT))
            {
                if (pctMoved >= PercentThreshold)
                {
                    screenOffsetCount -= 1;
                    swappedLoadingScreen = true;
                }
            }
            else if (info.Direction.HasFlag(SwipeDirection.RIGHT))
            {
                if (pctMoved >= PercentThreshold)
                {
                    swappedLoadingScreen = true;
                }
            }

            pos.x = screenOffsetCount * canvasWidth;

            if (pos.x > 0.0f)
                pos.x = 0.0f;

            m_RectTransform.anchoredPosition = pos;

            if (swappedLoadingScreen)
            {
                RequestSceneLoadEvent loadEvt = new RequestSceneLoadEvent();
                loadEvt.SceneIndex = m_NextScene + 1;
                loadEvt.Fire();
                m_CurrentScene = m_NextScene;
            }
            else
            {
                m_NextScene = m_CurrentScene;
                HidePages();
            }
        }

        private void On_SceneLoaded(SceneLoadedEvent info)
        {
            //if (m_CaptureTexture != null)
            //    RenderTexture.ReleaseTemporary(m_CaptureTexture);

            HidePages();
        }

        void AdvanceNextScene()
        {
            m_NextScene = m_CurrentScene + 1;
            if (m_NextScene >= Scenes.Count)
                m_NextScene = 0;
        }

        void RetractNextScene()
        {
            m_NextScene = m_CurrentScene - 1;

            if (m_NextScene < 0)
                m_NextScene = Scenes.Count - 1;
        }

    }
}