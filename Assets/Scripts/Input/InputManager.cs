using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using static UTB.EventSystem.InputEvents;

namespace UTB.Input
{


    [DefaultExecutionOrder(-1)]
    public class InputManager : MonoBehaviourSingletonPersistent<InputManager>
    {
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


        public DeviceOrientation DeviceOrientation => m_DeviceOrientation;

        public override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            if (m_UserControls == null)
                m_UserControls = new UserControls();

            m_UserControls.Enable();

            m_UserControls.Touch.PrimaryTouch.started += PrimaryTouch_started;
            m_UserControls.Touch.PrimaryTouch.canceled += PrimaryTouch_canceled;

            m_DeviceOrientation = UnityEngine.Input.deviceOrientation;

            OrientationChangedEvent evt = new OrientationChangedEvent();
            evt.OldOrientation = DeviceOrientation.Unknown;
            evt.NewOrientation = m_DeviceOrientation;
            evt.Fire();
        }

        private void OnDisable()
        {
            if (m_UserControls == null)
                return;

            m_UserControls.Disable();

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

                        //Debug.Log($"[InputManager]: Starting swipe event at {m_StartPosition}");

                        evt.StartPosition = m_StartPosition;
                        evt.Direction = direction;
                        //Debug.Log($"Starting {direction} swipe");
                        evt.Fire();
                        m_Swiping = true;
                    }
                }
            }
            DeviceOrientation currentOrientation;
#if UNITY_EDITOR
            currentOrientation = (Screen.height > Screen.width) ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
#else
            currentOrientation = UnityEngine.Input.deviceOrientation;
#endif
            if (m_DeviceOrientation != currentOrientation)
            {
                var oldOrientation = m_DeviceOrientation;
                m_DeviceOrientation = currentOrientation;

                //Debug.Log($"[InputManager]: Orientation changed from {oldOrientation} to {currentOrientation}");
                
                
                OrientationChangedEvent evt = new OrientationChangedEvent();
                evt.OldOrientation = oldOrientation;
                evt.NewOrientation = currentOrientation;                
                evt.Fire();
            }
        }

        private void PrimaryTouch_started(InputAction.CallbackContext ctx)
        {
            //Debug.Log($"[InputManager] Touch start from Actions");

            Vector2 currentPosition = m_UserControls.Touch.PrimaryPosition.ReadValue<Vector2>();

            // If this is the first event we need to go from the touchscreen legacy control
            // as the new input system is bugged and will always return the first touch at (0, 0)
            //if (m_IsFirstEvent)
            //{
            //    if (ETouch.activeTouches.Count > 0)
            //    {
            //        currentPosition = ETouch.activeTouches[0].screenPosition;
            //    }
            //    else
            //    {
            //        Debug.Log("[InputManager] Expected at least one touch to be active!");
            //    }
            //    m_IsFirstEvent = false;
            //    Debug.Log($"[InputManager] Changing tap position to {currentPosition}");
            //}

            m_StartPosition = currentPosition;
            m_LastPosition = currentPosition;
            m_Holding = true;

            StartTouchEvent evt = new StartTouchEvent();
            evt.Position = currentPosition;
            evt.Time = (float)ctx.time;
            evt.Fire();            
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
            /**
             * We always want to detect a swipe end if we have already started swiping.
             * For instance if the user starts a swipe left, goes all the way left and 
             * back to the starting position we want to let the system know that the 
             * swipe ended. Even if distance is bellow the threshold
             */
            if (!m_Swiping && Vector2.Distance(endPosition, m_StartPosition) < m_SwipeThreshold )
                return;

            // There was a swipe
            SwipeEndEvent swipeEvt = new SwipeEndEvent();
            swipeEvt.StartPosition = m_StartPosition;
            swipeEvt.Position = endPosition;
            swipeEvt.Delta = endPosition - m_StartPosition;

            var direction = swipeEvt.Delta.normalized;

            swipeEvt.Direction = GetSwipeDirection(direction);
            //Debug.Log($"[InputManager]: Ending {swipeEvt.Direction} swipe event from {m_StartPosition} to {endPosition}");

            swipeEvt.Fire();
        }
    }
}