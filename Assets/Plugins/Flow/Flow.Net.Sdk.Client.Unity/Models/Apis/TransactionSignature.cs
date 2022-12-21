using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    /// <summary>Base64 encoded signature.</summary>
    public partial class TransactionSignature
    {
        [JsonProperty("address", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Address { get; set; }

        [JsonProperty("key_index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Key_index { get; set; }

        [JsonProperty("signature", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Signature { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}