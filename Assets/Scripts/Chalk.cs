using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.Input;

using static UTB.EventSystem.InputEvents;

public class Chalk : MonoBehaviour
{
    private bool drawing;
    private Camera m_MainCamera;

    private Plane m_Plane;
    
    private Vector3 currentTarget;

    public float Speed = 5.0f;

    // Start is called before the first frame update
    void Awake()
    {
        m_MainCamera = Camera.main;

        //var normal = m_MainCamera.transform.position - transform.position;
        //normal = normal.normalized;

        // The plane's normal must always be the camera's -Forward,
        // so it is always parallel to the camera view, and facing
        // the camera
        m_Plane = new Plane(-m_MainCamera.transform.forward, transform.position);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        m_Plane.SetNormalAndPosition(-m_MainCamera.transform.forward, transform.position);

        if (!drawing)
            return;

        float step = Speed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, step);
    }

    public void SetTargetFromCursor(Vector2 cursor)
    {
        drawing = true;
        var screenPosition = new Vector3(cursor.x, cursor.y, 0);

        var ray = m_MainCamera.ScreenPointToRay(screenPosition);

        if (m_Plane.Raycast(ray, out float t))
        {
            Vector3 hitPoint = ray.GetPoint(t);

            currentTarget = hitPoint;
            return;
        }

        Debug.LogWarning("[Follow Cursor] We shouldn't have reached this!");
    }

    public void StopDrawing()
    {
        drawing = false;
    }

#if false
    private void OnDrawGizmos()
    {
        var normal = m_Plane.normal;
        var position = transform.position;

        var v3 = new Vector3();

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, Color.green);
        Debug.DrawLine(corner1, corner3, Color.green);
        Debug.DrawLine(corner0, corner1, Color.green);
        Debug.DrawLine(corner1, corner2, Color.green);
        Debug.DrawLine(corner2, corner3, Color.green);
        Debug.DrawLine(corner3, corner0, Color.green);
        Debug.DrawRay(position, normal, Color.red);


        var targetToCamera = m_MainCamera.transform.position - currentTarget;

        Debug.DrawRay(currentTarget, targetToCamera.normalized, Color.magenta);

    }
#endif
}
