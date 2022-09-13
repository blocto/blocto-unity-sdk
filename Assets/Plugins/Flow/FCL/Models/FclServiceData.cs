using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models
{
    public partial class FclServiceData
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public JObject PollingEmail { get; set; }

        [JsonProperty("signatures", NullValueHandling = NullValueHandling.Ignore)]
        public List<JObject> Signatures { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce { get; set; }
    }
}