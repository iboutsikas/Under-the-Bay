using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTB.Data
{
    [Serializable]
    public class Station
    {
        [SerializeField]
        public Guid Id;
        [SerializeField]
        public string Name;
        public DateTimeOffset LastUpdate;
        public List<BayData> Samples;
        public DateTimeOffset LastAttempt;
    }
}
