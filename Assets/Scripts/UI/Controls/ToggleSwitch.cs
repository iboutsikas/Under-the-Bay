using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UTB.UI
{
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public class SwitchToggledEvent : UnityEvent<bool>
        { }

        [SerializeField]
        private SwitchToggledEvent m_ToggledEvent = new SwitchToggledEvent();
        public SwitchToggledEvent onToggled => m_ToggledEvent;

        public RectTransform Knob;
        public Image ActiveMask;
        public float AnimationTime;
        public LeanTweenType TweenType;

        public bool Toggled { get { return m_Toggle; } }

        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        private void OnSwitchToggled()
        {
            Vector2 offset;
            if (m_Toggle)
            {
                offset = new Vector2(0, 0);
            }
            else
            {
                float offsetX = m_RectTransform.rect.width - Knob.rect.width;

                offset = new Vector2(offsetX, 0);
            }
            m_Toggle = !m_Toggle;

            LeanTween.move(Knob, offset, AnimationTime)
                    .setEase(TweenType)
                    .setOnComplete(() =>
                    {
                        ActiveMask.gameObject.SetActive(m_Toggle);
                    });

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSwitchToggled();

            m_ToggledEvent.Invoke(m_Toggle);
        }

        public void SetValue(bool state, bool fireEvent = false)
        {
            m_Toggle = state;

            if (fireEvent)
                m_ToggledEvent.Invoke(m_Toggle);
        }

        private RectTransform m_RectTransform;
        private bool m_Toggle;
    }
}
