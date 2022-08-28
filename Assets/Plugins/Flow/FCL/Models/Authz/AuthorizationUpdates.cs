using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthorizationUpdates
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        [JsonProperty("params")]
        public AuthorizationUpdatesParams Params { get; set; }
    }
}