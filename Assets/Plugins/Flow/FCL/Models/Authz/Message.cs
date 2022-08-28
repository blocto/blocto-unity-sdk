using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class Message
    {
        public Message()
        {
            Authorizations = new List<string>();
            Params = new List<object>();
        }
        
        [JsonProperty("cadence")]
        public string Cadence { get; set; }
        
        [JsonProperty("refBlock")]
        public string RefBlock { get; set; }
        
        [JsonProperty("computeLimit")]
        public int ComputeLimit  { get; set; }
        
        [JsonProperty("proposer")]
        public object Proposer { get; set; }
        
        [JsonProperty("payer")]
        public object Payer { get; set; }
        
        [JsonProperty("authorizations")]
        public List<string> Authorizations { get; set; }
        
        [JsonProperty("params")]
        public List<object> Params { get; set; }
        
        [JsonProperty("arguments")]
        public List<string> Arguments { get; set; }
    }
}