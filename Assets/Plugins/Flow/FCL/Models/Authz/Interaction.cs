using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public class Interaction
    {
        public Interaction()
        {
            Assigns = new JObject();
            Params = new object();
            Account = new BaseAccount();
        }
        
        [JsonProperty("tag")]
        public string Tag { get; set; }
        
        [JsonProperty("assigns")]
        public JObject Assigns { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("reason")]
        public string Reason { get; set; }
        
        [JsonProperty("accounts")]
        public Dictionary<string, JObject> Accounts { get; set; }
        
        [JsonProperty("params")]
        public object Params { get; set; }
        
        [JsonProperty("arguments")]
        public Dictionary<string, Argument> Arguments { get; set; }
        
        [JsonProperty("message")]
        public Message Message { get; set; }
        
        [JsonProperty("proposer")]
        public string Proposer { get; set; }
        
        [JsonProperty("authorizations")]
        public List<string> Authorizations { get; set; }
        
        [JsonProperty("payer")]
        public string Payer { get; set; }
        
        [JsonProperty("events")]
        public JObject Events { get; set; }
        
        [JsonProperty("account")]
        public BaseAccount Account { get; set; }
        
        [JsonProperty("collection")]
        public JObject Collection { get; set; }
        
        [JsonProperty("transaction")]
        public JObject Transaction { get; set; }
        
        [JsonProperty("block")]
        public JObject Block { get; set; }
    }
}