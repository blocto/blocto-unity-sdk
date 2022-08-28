using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class SignableParameter
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
        
        [JsonProperty("payerId")]
        public string PayerId { get; set; }
    }
}