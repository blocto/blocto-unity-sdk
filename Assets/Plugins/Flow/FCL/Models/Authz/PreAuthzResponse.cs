using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class PreAuthzResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("data")]
        public PreAuthzData PreAuthzData { get; set; } 
    }
}