using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authn
{
    public partial class Updates
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public AuthnParams Params { get; set; }
    }
}