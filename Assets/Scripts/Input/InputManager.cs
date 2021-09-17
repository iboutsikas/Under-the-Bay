using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UTB.EventSystem.InputEvents;

namespace UTB.Input
{


    [DefaultExecutionOrder(-1)]
    public class InputManager : MonoBehaviourSingletonPersistent<InputManager>
    {
        // We need to ignore the first even for now, as it always starts at (0, 0) which is wrong
        private bool m_IsFirstEvent = true;
        [SerializeField]
        private UserControls m_UserControls;
        [SerializeField]
        private float m_DistanceThreshold;
        [SerializeField]
        private float m_SwipeThreshold = 2.0f;
        [SerializeField, Range(0, 1)]
        private float m_DirectionThreshold = 0.9f;

        [SerializeField, Range(0, 1)]
        private float m_HorizontalSwipeStartThreshold = 0.01f;
        [SerializeField, Range(0, 1)]
        private float m_VerticalSwipeStartThreshold = 0.01f;

        private bool m_Holding = false;
        private bool m_Swiping = false;
        private Vector2 m_StartPosition;
        private DeviceOrientation m_DeviceOrientation;

        private Vector2 m_LastPosition;

        public override void Awake()
        {
            base.Awake();
            m_UserControls = new UserControls();
        }

        private void OnEnable()
        {
            m_UserControls.Enable();
        }

        private void OnDisable()
        {
            m_UserControls.Disable();
        }

        private void Start()
        {
            if (m_UserControls == null)
                m_UserControls = new UserControls();

            m_UserControls.Touch.PrimaryTouch.started += PrimaryTouch_started;
            m_UserControls.Touch.PrimaryTouch.canceled += PrimaryTouch_canceled;
        }

        private void OnDestroy()
        {
            m_UserControls.Touch.PrimaryTouch.started -= PrimaryTouch_started;
            m_UserControls.Touch.PrimaryTouch.canceled -= PrimaryTouch_canceled;
        }

        private void ProgressSwipe(Vector2 currentPosition)
        {
            if (Vector2.Distance(currentPosition, m_LastPosition) < m_DistanceThreshold)
            {
                return;
            }
            SwipeProgressEvent evt = new SwipeProgressEvent();
            evt.StartPosition = m_StartPosition;
            evt.Position = currentPosition;
            evt.Delta = currentPosition - m_LastPosition;
            evt.Fire();

            m_LastPosition = currentPosition;
        }

        private bool ShouldStartSwipe(SwipeDirection direction, ref Vector2 difference)
        {
            bool shouldStartSwipe = false;

            if (direction.HasFlag(SwipeDirection.UP) || direction.HasFlag(SwipeDirection.DOWN))
            {
                float pctMoved = difference.y / Screen.height;
                shouldStartSwipe = Mathf.Abs(pctMoved) >= m_VerticalSwipeStartThreshold;

            }
            else if (direction.HasFlag(SwipeDirection.LEFT) || direction.HasFlag(SwipeDirection.RIGHT))
            {
                float pctMoved = difference.x / Screen.width;
                shouldStartSwipe = Mathf.Abs(pctMoved) >= m_HorizontalSwipeStartThreshold;
            }

            return shouldStartSwipe;
        }

        private void FixedUpdate()
        {
            if (m_Holding)
            {
                var currentPosition = m_UserControls.Touch.PrimaryPosition.ReadValue<Vector2>();

                if (m_Swiping)
                {
                    // We progress the current swipe
                    ProgressSwipe(currentPosition);
                }
                else
                {
                    // We need to see if we should start a new swipe
                    var difference = currentPosition - m_StartPosition;

                    var direction = GetSwipeDirection(difference.normalized);

                    bool shouldStartSwipe = ShouldStartSwipe(direction, ref difference);

                    if (shouldStartSwipe)
                    {
                        SwipeStartEvent evt = new SwipeStartEvent();

                        evt.StartPosition = m_StartPosition;
                        evt.Direction = direction;
                        //Debug.Log($"Starting {direction} swipe");
                        evt.Fire();
                        m_Swiping = true;
                    }
                }
            }

            if (m_DeviceOrientation != UnityEngine.Input.deviceOrientation)
            {
                OrientationChangedEvent evt = new OrientationChangedEvent();
                evt.OldOrientation = m_DeviceOrientation;
                evt.NewOrientation = UnityEngine.Input.deviceOrientation;
                evt.Fire();

                m_DeviceOrientation = UnityEngine.Input.deviceOrientation;
            }
        }

        private void PrimaryTouch_started(InputAction.CallbackContext ctx)
        {
            if (m_IsFirstEvent)
            {
                m_IsFirstEvent = false;
                return;
            }

            StartTouchEvent evt = new StartTouchEvent();
            evt.Position = m_UserControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            evt.Time = (float)ctx.time;
            evt.Fire();

            m_StartPosition = evt.Position;
            m_LastPosition = evt.Position;
            m_Holding = true;
        }

        private void PrimaryTouch_canceled(InputAction.CallbackContext ctx)
        {
            var endPosition = m_UserControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            EndTouchEvent evt = new EndTouchEvent();
            evt.Position = endPosition;
            evt.Time = (float)ctx.time;
            evt.Fire();

            m_Holding = false;

            DetectSwipe(endPosition);
            m_Swiping = false;
        }

        private SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            SwipeDirection swipeDir = SwipeDirection.NONE;
            if (Vector2.Dot(Vector2.up, direction) > m_DirectionThreshold)
            {
                swipeDir |= SwipeDirection.UP;
            }

            if (Vector2.Dot(Vector2.right, direction) > m_DirectionThreshold)
            {
                swipeDir |= SwipeDirection.RIGHT;
            }

            if (Vector2.Dot(Vector2.down, direction) > m_DirectionThreshold)
            {
                swipeDir |= SwipeDirection.DOWN;
            }

            if (Vector2.Dot(Vector2.left, direction) > m_DirectionThreshold)
            {
                swipeDir |= SwipeDirection.LEFT;
            }
            return swipeDir;
        }

        private void DetectSwipe(Vector2 endPosition)
        {
            if (Vector2.Distance(endPosition, m_StartPosition) < m_SwipeThreshold)
                return;

            // There was a swipe
            SwipeEndEvent swipeEvt = new SwipeEndEvent();
            swipeEvt.StartPosition = m_StartPosition;
            swipeEvt.Position = endPosition;
            swipeEvt.Delta = endPosition - m_StartPosition;

            var direction = swipeEvt.Delta.normalized;

            swipeEvt.Direction = GetSwipeDirection(direction);
            //Debug.Log($"Finished {swipeEvt.Direction} swipe");

            swipeEvt.Fire();
        }
    }
}