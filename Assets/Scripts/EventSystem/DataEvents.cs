using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTB.EventSystem;

namespace UTB.EventSystem
{
    public static class DataEvents
    {
        public class RequestLoadStationsEvent: Event<RequestLoadStationsEvent> { }

        public class StationsLoadedEvent : Event<StationsLoadedEvent> { }

        public class SelectActiveStationEvent: Event<SelectActiveStationEvent> {
            public Guid Guid;
            public DateTimeOffset? From;
            public DateTimeOffset? To;

            public SelectActiveStationEvent()
            {
                Guid = Guid.Empty;
                From = null;
                To = null;
            }
        }

        public class ActiveStationChangedEvent : Event<ActiveStationChangedEvent> {
            public Guid Guid;
        }

        public class RequestLoadSamplesEvent: Event<RequestLoadSamplesEvent>
        {
            public Guid StationId;
            public DateTimeOffset? From;
            public DateTimeOffset? To;

            public RequestLoadSamplesEvent()
            {
                StationId = Guid.Empty;
                From = To = null;
            }
        }

        public class SamplesLoadedEvent : Event<SamplesLoadedEvent> { }

        public class SelectActiveSampleEvent: Event<SelectActiveSampleEvent>
        {
            public int SampleIndex;
        }
        

        public class SampleDatesChangedEvent: Event<SampleDatesChangedEvent>
        {
            public DateTimeOffset? From;
            public DateTimeOffset? To;

            public SampleDatesChangedEvent()
            {
                From = null;
                To = null;
            }
        }
    }
}
