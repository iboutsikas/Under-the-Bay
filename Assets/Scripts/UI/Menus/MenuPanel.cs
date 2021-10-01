using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.UI
{
    public enum MenuType
    {
        NONE = 0,
        MAIN_MENU,
        SYSTEM,
        MAP,
        ABOUT,
        COUNT
    };

    public class MenuPanel : MonoBehaviour
    {
        protected MenuManager m_MenuManager;
        protected MenuType m_Type = MenuType.NONE;

        private RectTransform m_RectTransform;
        private bool m_IsOpen = false;

        public MenuType MenuType { get { return m_Type; } }
        public float animationTime = 0.5f;
        public LeanTweenType TweenType;

        public float OriginalY = 0;
        public float HiddenY = -744;

        public MenuPanel(MenuType type)
        {
            m_Type = type;
        }

        protected virtual void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_RectTransform.anchoredPosition = new Vector2(0, HiddenY);
            
            m_IsOpen = false;
            gameObject.SetActive(false);
            
            m_MenuManager = MenuManager.Instance;
            m_MenuManager.RegisterMenu(this);
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
            this.m_MenuManager.RequestClose(this);
        }
    }

}
