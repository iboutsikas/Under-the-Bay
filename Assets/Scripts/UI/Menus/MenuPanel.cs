using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class MenuPanel : MonoBehaviour
    {
        protected MenuManager m_MenuManager;

        private RectTransform m_RectTransform;
        private bool m_IsOpen = false;


        public float animationTime = 0.5f;
        public LeanTweenType TweenType;

        public float OriginalY = 0;
        public float HiddenY = -744;

        protected virtual void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_RectTransform.anchoredPosition = new Vector2(0, HiddenY);
            
            m_IsOpen = false;
            gameObject.SetActive(false);
        }

        public void PopIn(Action callback = null)
        {
            gameObject.SetActive(true);

            var desc = LeanTween.move(m_RectTransform, new Vector2(0.0f, OriginalY), animationTime)
                .setEase(TweenType);

            if (callback != null)
                desc.setOnComplete(callback);
                
            //m_RectTransform.rect.bottom = 0;
            m_IsOpen = true;
        }

        public void PopOut(Action callback = null)
        {
            var desc = LeanTween.move(m_RectTransform, new Vector2(0.0f, HiddenY), animationTime)
                .setEase(TweenType)
                .setOnComplete(() =>
                {
                    m_IsOpen = false;
                    gameObject.SetActive(false);
                });

            if (callback != null)
                desc.setOnComplete(callback);
        }

        public bool IsOpen()
        {
            return m_IsOpen;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void CloseMenu()
        {
            this.PopOut();
        }

        public void RegisterManager(MenuManager manager)
        {
            m_MenuManager = manager;
        }
    }

}
