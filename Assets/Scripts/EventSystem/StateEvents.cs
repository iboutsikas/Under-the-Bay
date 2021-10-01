using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.EventSystem
{
    public static class StateEvents
    {
        public class DataStreamToggledEvent: Event<DataStreamToggledEvent>
        {
            public bool StreamEnabled;
        }

        public class StoriesToggledEvent: Event<StoriesToggledEvent>
        {
            public bool StoriesEnabled;
        }

        public class VoiceMixToggledEvent: Event<VoiceMixToggledEvent>
        {
            public bool MixEnabled;
        }

        public class VoiceMixChangedEvent: Event<VoiceMixChangedEvent>
        {
            public float Value;
        }
    }
}