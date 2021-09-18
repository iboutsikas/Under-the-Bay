using Newtonsoft.Json;
using System;

namespace UTB.Data
{
    [Serializable]
    public class BayData
    {
        public DateTimeOffset SampleDate;
        [JsonProperty("dissolvedOxygen")]
        public float Oxygen;
        [JsonProperty("waterTemperature")]
        public float Temperature;
        public float Salinity;
        public float Turbidity;
        public float PH;
        public float Chlorophyll;
    };
}
