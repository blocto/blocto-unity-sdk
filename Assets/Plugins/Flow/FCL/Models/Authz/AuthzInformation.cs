using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class AuthzInformation
    {
        [JsonProperty("id")]
        public string id { get; set; }
        
        [JsonProperty("identity")]
        public PreAuthzIdentity Identity { get; set; }
        
        [JsonProperty("method")]
        public string Method { get; set; }
        
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        
        [JsonProperty("params")]
        public SignableParameter Params { get; set; }
    }
}