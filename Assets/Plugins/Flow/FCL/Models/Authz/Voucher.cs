using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public partial class Voucher
    {
        public Voucher()
        {
            Authorizers = new List<string>();
            PayloadSigs = new List<JObject>();
            EnvelopeSigs = new List<JObject>
                           {
                               new JObject()
                           };
            ProposalKey = new ProposalKey();
        }
        
        [JsonProperty("cadence")]
        public string Cadence { get; set; }

        [JsonProperty("refBlock")]
        public object RefBlock { get; set; }

        [JsonProperty("computeLimit")]
        public long ComputeLimit { get; set; }

        [JsonProperty("arguments")]
        public List<BaseArgument> Arguments { get; set; }

        [JsonProperty("proposalKey")]
        public ProposalKey ProposalKey { get; set; }

        [JsonProperty("payer")]
        public object Payer { get; set; }

        [JsonProperty("authorizers")]
        public List<string> Authorizers { get; set; }

        [JsonProperty("payloadSigs")]
        public List<JObject> PayloadSigs { get; set; }

        [JsonProperty("envelopeSigs")]
        public List<JObject> EnvelopeSigs { get; set; }
    }
}