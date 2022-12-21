using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class AggregatedSignature
    {
        [JsonProperty("verifier_signatures", Required = Required.Always)]
        public ICollection<byte[]> Verifier_signatures { get; set; } = new System.Collections.ObjectModel.Collection<byte[]>();

        [JsonProperty("signer_ids", Required = Required.Always)]
        public ICollection<string> Signer_ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        } 
    }
}