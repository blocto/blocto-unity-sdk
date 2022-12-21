using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class Transaction
    {
        [JsonProperty("id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Id { get; set; }

        /// <summary>Base64 encoded Cadence script.</summary>
        [JsonProperty("script", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Script { get; set; }

        /// <summary>Array of Base64 encoded arguments with in [JSON-Cadence interchange format](https://docs.onflow.org/cadence/json-cadence-spec/).</summary>
        [JsonProperty("arguments", Required = Required.Always)]
        [Required]
        public ICollection<byte[]> Arguments { get; set; } = new System.Collections.ObjectModel.Collection<byte[]>();

        [JsonProperty("reference_block_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Reference_block_id { get; set; }

        /// <summary>The limit on the amount of computation a transaction is allowed to preform.</summary>
        [JsonProperty("gas_limit", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Gas_limit { get; set; }

        [JsonProperty("payer", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Payer { get; set; }

        [JsonProperty("proposal_key", Required = Required.Always)]
        [Required]
        public ProposalKey Proposal_key { get; set; } = new ProposalKey();

        [JsonProperty("authorizers", Required = Required.Always)]
        [Required]
        public ICollection<string> Authorizers { get; set; } = new System.Collections.ObjectModel.Collection<string>();

        [JsonProperty("payload_signatures", Required = Required.Always)]
        [Required]
        public ICollection<TransactionSignature> Payload_signatures { get; set; } = new System.Collections.ObjectModel.Collection<TransactionSignature>();

        [JsonProperty("envelope_signatures", Required = Required.Always)]
        [Required]
        public ICollection<TransactionSignature> Envelope_signatures { get; set; } = new System.Collections.ObjectModel.Collection<TransactionSignature>();

        [JsonProperty("result", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public TransactionResult Result { get; set; }

        [JsonProperty("_expandable", Required = Required.Always)]
        [Required]
        public _expandable3 _expandable { get; set; } = new _expandable3();

        [JsonProperty("_links", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Links _links { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }


    }
}