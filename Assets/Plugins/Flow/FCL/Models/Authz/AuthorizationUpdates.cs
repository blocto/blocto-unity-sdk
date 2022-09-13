using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthorizationUpdates
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        [JsonProperty("params")]
        public JObject Params { get; set; }
    }
}