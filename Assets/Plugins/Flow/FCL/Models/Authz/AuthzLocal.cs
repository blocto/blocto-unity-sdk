using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public partial class AuthzLocal
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("params")]
        public JObject Params { get; set; }
    }
}