using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class SignableService
    {
        [JsonProperty("params")]
        public SignableParameter Parameter { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}