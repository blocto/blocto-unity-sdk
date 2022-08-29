using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class Role
    {
        [JsonProperty("proposer")]
        public bool Proposer { get; set; }
        
        [JsonProperty("authorizer")]
        public bool Authorizer { get; set; }
        
        [JsonProperty("payer")]
        public bool Payer { get; set; }
    }
}