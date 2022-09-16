using System.Collections.Generic;
using Flow.FCL.Models.Authz;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models
{
    public class SignMessageResponse : IResponse
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }
        
        [JsonProperty("status")]
        public ResponseStatusEnum ResponseStatus { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("data")]
        
        public List<JObject> Data { get; set; }
    }
}