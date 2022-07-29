using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Blocto.Flow.Client.Http.Models.Apis
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class TransactionBody
    {
        /// <summary>Base64 encoded content of the Cadence script.</summary>
        [JsonProperty("script", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Script { get; set; }

        /// <summary>A list of arguments each encoded as Base64 passed in the [JSON-Cadence interchange format](https://docs.onflow.org/cadence/json-cadence-spec/).</summary>
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

        /// <summary>A list of Base64 encoded signatures.</summary>
        [JsonProperty("payload_signatures", Required = Required.Always)]
        [Required]
        public ICollection<TransactionSignature> Payload_signatures { get; set; } = new System.Collections.ObjectModel.Collection<TransactionSignature>();

        /// <summary>A list of Base64 encoded signatures.</summary>
        [JsonProperty("envelope_signatures", Required = Required.Always)]
        [Required]
        public ICollection<TransactionSignature> Envelope_signatures { get; set; } = new System.Collections.ObjectModel.Collection<TransactionSignature>();

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}