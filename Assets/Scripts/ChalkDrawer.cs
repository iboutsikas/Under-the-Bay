using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UTB.Input;
using UTB.UI;
using static UTB.EventSystem.InputEvents;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class ChalkDrawer : MonoBehaviour
{
    [NonSerialized] private Chalk selection = null;

    [NonSerialized] private Camera mainCamera;

    [NonSerialized] private PageSwiper swiper;

    public LayerMask Layer;

    private void Awake()
    {
        mainCamera = Camera.main;
        Debug.Assert(mainCamera != null);

        swiper = GameObject.FindObjectOfType<PageSwiper>(true);
        Debug.Assert(swiper != null);
    }

    private void OnEnable()
    {
        StartTouchEvent.Subscribe(On_TouchStart);
        EndTouchEvent.Subscribe(On_TouchEnd);
    }

    private void OnDisable()
    {
        StartTouchEvent.Unsubscribe(On_TouchStart);
        EndTouchEvent.Unsubscribe(On_TouchEnd);
    }
    
    private void FixedUpdate()
    {
        if (selection == null)
            return;

        var screenPos = InputManager.Instance.CursorPosition;

        selection.SetTargetFromCursor(screenPos);
    }

    private void On_TouchStart(StartTouchEvent info)
    {
        if (info.OverUI)
            return;

        var ray = mainCamera.ScreenPointToRay(info.Position);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Layer.value))
        {
            return;
        }

        selection = hit.collider.gameObject.GetComponent<Chalk>();
        Debug.Assert(selection != null);
        swiper.Disable();
    }

    private void On_TouchEnd(EndTouchEvent info)
    {
        if (selection != null)
        {
            selection.StopDrawing();
            swiper.Enable();
        }
        selection = null;
    }
}
