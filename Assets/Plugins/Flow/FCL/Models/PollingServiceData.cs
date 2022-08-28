using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingServiceData
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public PollingEmail PollingEmail { get; set; }

        [JsonProperty("signatures", NullValueHandling = NullValueHandling.Ignore)]
        public List<CompositeSignature> Signatures { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce { get; set; }
    }
}