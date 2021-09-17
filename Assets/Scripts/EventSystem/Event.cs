using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UTB.EventSystem
{
    public abstract class Event<T> where T : Event<T>
    {
        #region Event Implementation
        public delegate void EventListener(T info);
        private static event EventListener listeners;

        public static void Subscribe(EventListener listener)
        {
            listeners += listener;
        }

        public static void Unsubscribe(EventListener listener)
        {
            listeners -= listener;
        }
        #endregion

        #region Instance Implementation
        public string Description;

        private bool m_HasFired = false;

        public void Fire()
        {
            if (m_HasFired)
                throw new Exception("The event has already fired!");

            m_HasFired = true;
            if (listeners != null)
            {
                //var type = this.GetType();
                //Debug.Log($"[{type.Name}] Number of listeners: {listeners.GetInvocationList().Length}");
                listeners(this as T);
            }
        }
        #endregion

    }
}