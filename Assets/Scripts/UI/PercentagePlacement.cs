using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UTB.EventSystem.InputEvents;
namespace UTB.UI
{
    [ExecuteInEditMode]
    public class PercentagePlacement : UIBehaviour
    {
        private bool m_IgnoreNextEvent = false;
        private string m_DebugString;
        private RectTransform m_RectTransform;

        [Header("Placement")]
        [Range(0, 100)]
        public float Top = 0.0f;
        [Range(0, 100)]
        public float Left = 0.0f;

        [Header("Size")]
        public bool OverrideWidth = true;
        [Range(0, 100)]
        public float Width = 100.0f;
        public bool UseMaxWidth = false;
        public float MaxWidth;

        public bool OverrideHeight = true;
        [Range(0, 100)]
        public float Height = 100.0f;
        public bool UseMaxHeight = false;
        public float MaxHeight;

        protected override void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var pos = transform.position;
            pos.y += 20;
            Handles.Label(pos, m_DebugString);
        }
#endif

#if UNITY_EDITOR
        private void Update()
#else
        private void FixedUpdate()
#endif
        {
            m_IgnoreNextEvent = true;
            Recalculate();
        }

        void Recalculate(bool flip = false)
        {
            var parent = m_RectTransform.parent.GetComponent<RectTransform>();

            // When we get the orientation changed event, the canvas has not
            // been recalculated yet. So we need to flip the width and height
            // 
            var parentHeight = flip ? parent.rect.width : parent.rect.height;
            var parentWidth = flip ? parent.rect.height : parent.rect.width;

            var myLeft = parentWidth * (Left * 0.01f);            
            var myTop = parentHeight * -(Top * 0.01f);

            var myWidth = OverrideWidth ? parentWidth * (Width * 0.01f) : m_RectTransform.rect.width;
            if (UseMaxWidth)
            {
                myWidth = Mathf.Min(myWidth, MaxWidth);
            }

            var myHeight = OverrideHeight ? parentHeight * (Height * 0.01f) : m_RectTransform.rect.height;
            if (UseMaxHeight)
            {
                myHeight = Mathf.Min(myHeight, MaxHeight);
            }

            m_RectTransform.anchoredPosition = new Vector2(myLeft, myTop);
            m_RectTransform.sizeDelta = new Vector2(myWidth, myHeight);
            m_DebugString = $"Left: {myLeft:F2}, Top: {myTop:F2}, Width: {myWidth:F2}, Height: {myHeight:F2}";
        }

        

        protected override void OnRectTransformDimensionsChange()
        {
            if (m_RectTransform == null)
                return;

            if (m_IgnoreNextEvent)
            {
                m_IgnoreNextEvent = false;
                return;
            }

            Recalculate();
        }
    }
}