using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class PreAuthzIdentity
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        
        [JsonProperty("keyId")]
        public int KeyId { get; set; }
    }
}