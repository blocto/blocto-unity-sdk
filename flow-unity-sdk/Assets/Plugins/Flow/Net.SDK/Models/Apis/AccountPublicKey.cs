using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Blocto.Flow.Client.Http.Models.Enums;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Http.Models.Apis
{
    public partial class AccountPublicKey
    {
        /// <summary>Index of the public key.</summary>
        [JsonProperty("index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Index { get; set; }

        /// <summary>Hex encoded public key.</summary>
        [JsonProperty("public_key", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Public_key { get; set; }

        [JsonProperty("signing_algorithm", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public SigningAlgorithm Signing_algorithm { get; set; }

        [JsonProperty("hashing_algorithm", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public HashingAlgorithm Hashing_algorithm { get; set; }

        /// <summary>Current account sequence number.</summary>
        [JsonProperty("sequence_number", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Sequence_number { get; set; }

        /// <summary>Weight of the key.</summary>
        [JsonProperty("weight", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Weight { get; set; }

        /// <summary>Flag indicating whether the key is active or not.</summary>
        [JsonProperty("revoked", Required = Required.Always)]
        public bool Revoked { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}