using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class PreAuthzData
    {
        [JsonProperty("f_type")]
        public string F_Type { get; set; }
        
        [JsonProperty("f_vsn")]
        public string F_Vsn { get; set; }
        
        [JsonProperty("proposer")]
        public AuthzInformation Proposer { get; set; }
        
        [JsonProperty("payer")]
        public List <AuthzInformation> Payer { get; set; }
        
        [JsonProperty("authorization")]
        public List <AuthzInformation> Authorization { get; set; }
    }
}