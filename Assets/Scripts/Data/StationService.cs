using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace UTB.Data
{   
    //[Serializable]
    public class StationService
    {
        private static string api = "https://bay-db.irc.umbc.edu";
        //private static string api = "http://localhost:8080";

        public List<Station> GetAllStations()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(api + "/api/V1/stations");
            request.Method = "GET";
            request.Accept = "application/json";

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var result = JsonConvert.DeserializeObject<List<Station>>(json);

                return result;
            }
        }

        public List<BayData> GetSamples(Guid guid, DateTimeOffset from, DateTimeOffset to)
        {

            if (guid == Guid.Empty)
            {
                Debug.DebugBreak();
            }

            var fromDateString = from.ToString("yyyy-MM-ddTHH:mm:sszzzz");
            var toDateString = to.ToString("yyyy-MM-ddTHH:mm:sszzzz");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(api + $"/api/V1/stations/{guid}?include_measurements=true" +
                $"&start_date={Uri.EscapeDataString(fromDateString)}&end_date={Uri.EscapeDataString(toDateString)}"
            );

            request.Method = "GET";
            request.Accept = "application/json";

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<Station>(json);

                    return result.Samples;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                Debug.LogError($"Url was {request.RequestUri}");

                throw new AggregateException($"Request for {request.RequestUri} failed. See inner exception for details.", e);                
            }
        }
    }
}
