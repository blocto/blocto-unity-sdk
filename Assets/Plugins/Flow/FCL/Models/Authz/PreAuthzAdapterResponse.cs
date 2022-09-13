using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class PreAuthzAdapterResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("data")]
        public AuthorizerData AuthorizerData { get; set; } 
    }
}