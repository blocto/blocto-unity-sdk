using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class AuthorizerData
    {
        [JsonProperty("f_type")]
        public string F_Type { get; set; }
        
        [JsonProperty("f_vsn")]
        public string F_Vsn { get; set; }
        
        [JsonProperty("proposer")]
        public AuthInformation Proposer { get; set; }
        
        [JsonProperty("payer")]
        public List <AuthInformation> Payers { get; set; }
        
        [JsonProperty("authorization")]
        public List <AuthInformation> Authorizations { get; set; }
    }
}