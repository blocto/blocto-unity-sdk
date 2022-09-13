using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public class AuthInformation
    {
        [JsonProperty("id")]
        public string id { get; set; }
        
        [JsonProperty("identity")]
        public Identity Identity { get; set; }
        
        [JsonProperty("method")]
        public string Method { get; set; }
        
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        
        [JsonProperty("params")]
        public JObject Params { get; set; }
    }
}