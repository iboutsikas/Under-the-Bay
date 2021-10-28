using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.EventSystem
{

    public static class InputEvents
    {
        public class StartTouchEvent : Event<StartTouchEvent>
        {
            public Vector2 Position;
            public float Time;
            public bool OverUI;
        }

        public class EndTouchEvent : Event<EndTouchEvent>
        {
            public Vector2 Position;
            public float Time;
        }

        public class SwipeStartEvent : Event<SwipeStartEvent>
        {
            public Vector2 StartPosition;
            public SwipeDirection Direction;
        }

        public class SwipeProgressEvent : Event<SwipeProgressEvent>
        {
            public Vector2 StartPosition;
            public Vector2 Position;
            public Vector2 Delta;
        }

        public class SwipeEndEvent : Event<SwipeEndEvent>
        {
            public Vector2 StartPosition;
            public Vector2 Position;
            public Vector2 Delta;
            public SwipeDirection Direction;
        }

        public class OrientationChangedEvent : Event<OrientationChangedEvent>
        {
            public DeviceOrientation OldOrientation;
            public DeviceOrientation NewOrientation;
        }

        [System.Flags]
        public enum SwipeDirection
        {
            NONE = 0x00,
            UP = 0x01,
            RIGHT = 0x02,
            DOWN = 0x04,
            LEFT = 0x08,
            UNKNOWN = 0x16
        };
    }
}
