using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UTB.Core;
using static MenuEvents;
using static UTB.EventSystem.InputEvents;
using static UTB.EventSystem.SceneLoadingEvents;

namespace UTB.UI
{
    public class PageSwiper : MonoBehaviour
    {
        private bool m_OnHomeScreen = false;
        private bool m_Swiping = false;
        private bool m_WaitingForLoad = false;
        private bool m_WaitingForFade = false;

        private RectTransform m_RectTransform;
        private Page[] m_Pages;
        private RenderTexture m_CaptureTexture;
        private int m_CurrentScene = 0;
        private int m_NextScene = 1;

        [SerializeField]
        private float m_FadeoutDelay = 1.0f;
        [SerializeField]
        private float m_SlideDuration = 1.0f;
        [SerializeField]
        private LeanTweenType m_SlideEaseType;

        [Range(0, 1)]
        public float PercentThreshold = 0.8f;
        [Range(0, 1)]
        public float ElasticDragThreshold = 0.05f;

        public RectTransform Canvas;
        public SceneConfiguration SceneConfig;

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_Pages = GetComponentsInChildren<Page>();

            SceneLoadedEvent.Subscribe(On_SceneLoaded);

            MenuOpenedEvent.Subscribe(On_MenuOpened);
            MenuClosedEvent.Subscribe(On_MenuClosed);

            // We will only be activated once AR scenes have loaded
            Disable();
        }

        private void OnDestroy()
        {
            SceneLoadedEvent.Unsubscribe(On_SceneLoaded);
            MenuOpenedEvent.Unsubscribe(On_MenuOpened);
            MenuClosedEvent.Unsubscribe(On_MenuClosed);
        }

        private void Start()
        {
            foreach (var page in m_Pages)
            {
                page.Resize(Canvas.rect.width, Canvas.rect.height, DeviceOrientation.Unknown);
            }

            HidePages();
        }

        private void OnEnable()
        {
            SwipeStartEvent.Subscribe(On_SwipeStart);
            SwipeProgressEvent.Subscribe(On_SwipeProgress);
            SwipeEndEvent.Subscribe(On_SwipeEnd);
            OrientationChangedEvent.Subscribe(On_OrientationChange);

        }

        private void OnDisable()
        {
            SwipeStartEvent.Unsubscribe(On_SwipeStart);
            SwipeProgressEvent.Unsubscribe(On_SwipeProgress);
            SwipeEndEvent.Unsubscribe(On_SwipeEnd);
            OrientationChangedEvent.Unsubscribe(On_OrientationChange);
        }

        private void On_OrientationChange(OrientationChangedEvent info)
        {
            //Debug.Log($"[PageSwiper] Orientation changed to: {info.NewOrientation}");
            //Debug.Log($"[PageSwiper] Canvas width: {Canvas.rect.width}, Canvas height: {Canvas.rect.height}");

            foreach (var page in m_Pages)
            {
                page.Resize(Canvas.rect.height, Canvas.rect.width, info.NewOrientation);
            }
        }

        private void ShowPages()
        {
            foreach (var page in m_Pages)
            {
                page.Show();
            }
        }

        private void HidePages()
        {
            foreach (var page in m_Pages)
            {
                page.Hide();
            }
        }

        private void FadeoutPages()
        {
            foreach(var page in m_Pages)
            {
                page.FadeOut();
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
            bool isCorrectDirectionSwipe = 
                info.Direction.HasFlag(SwipeDirection.LEFT) || 
                info.Direction.HasFlag(SwipeDirection.RIGHT);

            bool waiting = m_WaitingForFade || m_WaitingForLoad;
            
            if (m_Swiping || waiting || !isCorrectDirectionSwipe)
                return;
            
            m_Swiping = true;

            m_CaptureTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);

            Debug.Log($"Screen.width: {Screen.width}, Screen.height: {Screen.height}");

            ScreenCapture.CaptureScreenshotIntoRenderTexture(m_CaptureTexture);

            if (info.Direction.HasFlag(SwipeDirection.LEFT))
            {
                AdvanceNextScene();
                m_Pages[0].EnableCaptureScreen(m_CaptureTexture, Canvas.rect.width, Canvas.rect.height);
                m_Pages[1].EnableLoadingScreen(SceneConfig.ARScenes[m_NextScene]);

                MoveFullRight();
            }
            else
            {
                AdvancePreviousScene();
                m_Pages[0].EnableLoadingScreen(SceneConfig.ARScenes[m_NextScene]);
                m_Pages[1].EnableCaptureScreen(m_CaptureTexture, Canvas.rect.width, Canvas.rect.height);

                MoveFullLeft();
            }

            ShowPages();
        }

        private void On_SwipeProgress(SwipeProgressEvent evt)
        {
            bool waiting = m_WaitingForFade || m_WaitingForLoad;

            if (!m_Swiping || waiting)
                return;

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
            bool waiting = m_WaitingForFade || m_WaitingForLoad;

            // We immediately go to swipe processing if we are already swiping
            if (m_Swiping && !waiting)
                goto Swipe;

            bool isCorrectDirectionSwipe =
                info.Direction.HasFlag(SwipeDirection.LEFT) ||
                info.Direction.HasFlag(SwipeDirection.RIGHT);


            if (!isCorrectDirectionSwipe || waiting)
                return;

            Swipe:
            m_Swiping = false;

            var pos = m_RectTransform.anchoredPosition;
            var canvasWidth = Canvas.rect.width;

            float difference = (info.StartPosition - info.Position).x;

            // Takes a value from -1, 0 basically, so we need to clamp it next
            int screenOffsetCount = Mathf.RoundToInt(pos.x / canvasWidth);

            if (screenOffsetCount <= -m_Pages.Length) 
            { 
                screenOffsetCount = -(m_Pages.Length - 1);
            }

            if (screenOffsetCount > 0)
            {
                screenOffsetCount = 0;
            }

            float pctMoved = Mathf.Abs(difference / canvasWidth);

            bool swappedLoadingScreen = pctMoved >= PercentThreshold;


            pos.x = screenOffsetCount * canvasWidth;

            if (pos.x > 0.0f)
                pos.x = 0.0f;

            // NOTE: on complete will activate at a later point
            // probably after the load has been requested
            var desc = LeanTween.move(m_RectTransform, pos, m_SlideDuration)
                .setEase(m_SlideEaseType)
                .setOnComplete(() =>
                {
                    if (swappedLoadingScreen)
                        StartFadeoutTimer();
                    else
                        HidePages();
                });

            if (swappedLoadingScreen)
            {
                m_WaitingForLoad = true;
                m_WaitingForFade = true;

                RequestSceneLoadEvent loadEvt = new RequestSceneLoadEvent();
                loadEvt.SceneIndex = SceneConfig.ARScenes[m_NextScene].BuildIndex;
                loadEvt.Fire();
                m_CurrentScene = m_NextScene;
            }
            else
            {
                m_NextScene = m_CurrentScene;
            }
        }

        private void On_SceneLoaded(SceneLoadedEvent info)
        {
            if (info.BuildIndex == SceneConfig.HomeScreen.BuildIndex)
            {
                m_OnHomeScreen = true;
                Disable();
                return;
            }

            m_OnHomeScreen = false;
            m_WaitingForLoad = false;

            if (!gameObject.activeSelf)
            {
                Enable();
            }

            // Update the scene to whatever was loaded
            for(int i = 0; i < SceneConfig.ARScenes.Count; ++i)
            {
                if (SceneConfig.ARScenes[i].BuildIndex == info.BuildIndex)
                {
                    m_CurrentScene = i;
                    break;
                }
            }
            
            if (!m_WaitingForFade)
                FadeoutPages();
        }

        private void On_MenuOpened(MenuOpenedEvent info)
        {
            if (this.gameObject.activeSelf)
                Disable();
        }

        private void On_MenuClosed(MenuClosedEvent info)
        {
            if (!this.gameObject.activeSelf && !m_OnHomeScreen)
                Enable();
        }

        void AdvanceNextScene()
        {
            m_NextScene = m_CurrentScene + 1;
            if (m_NextScene >= SceneConfig.ARScenes.Count)
                m_NextScene = 0;
        }

        void AdvancePreviousScene()
        {
            m_NextScene = m_CurrentScene - 1;
            if (m_NextScene < 0)
                m_NextScene = SceneConfig.ARScenes.Count - 1;
        }

        void StartFadeoutTimer()
        {
            m_WaitingForFade = true;
            StartCoroutine(fadeOut());
        }

        IEnumerator fadeOut()
        {
            yield return new WaitForSeconds(m_FadeoutDelay);
            
            m_WaitingForFade = false;

            if (!m_WaitingForLoad)
                FadeoutPages();
        }

    }
}