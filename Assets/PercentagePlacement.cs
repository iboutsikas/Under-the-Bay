using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class PercentagePlacement : UIBehaviour
{
#if UNITY_EDITOR
    private bool m_IgnoreNextEvent = false;
#endif
    private RectTransform m_RectTransform;

    [Header("Placement")]
    [Range(0, 100)]
    public float Top = 0.0f;
    [Range(0, 100)]
    public float Left = 0.0f;

    [Header("Size")]
    [Range(0, 100)]
    public float Width = 100.0f;
    [Range(0, 100)]
    public float Height = 100.0f;

    protected override void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

#if UNITY_EDITOR
    private void Update()
    {
        m_IgnoreNextEvent = true;

        Recalculate();
    }
#endif

    void Recalculate()
    {
        var parent = m_RectTransform.parent.GetComponent<RectTransform>();

        var parentHeight = parent.rect.height;
        var parentWidth = parent.rect.width;

        var myLeft = parentWidth * (Left * 0.01f);
        var myTop = parentHeight * -(Top * 0.01f);

        var myWidth = parentWidth * (Width / 100.0f);
        var myHeight = parentHeight * (Height / 100.0f);

        m_RectTransform.anchoredPosition = new Vector2(myLeft, myTop);
        m_RectTransform.sizeDelta = new Vector2(myWidth, myHeight);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (m_RectTransform == null)
            return;

#if UNITY_EDITOR
        if (m_IgnoreNextEvent)
        {
            m_IgnoreNextEvent = false;
            return;
        }
#endif

        Recalculate();
    }
}
