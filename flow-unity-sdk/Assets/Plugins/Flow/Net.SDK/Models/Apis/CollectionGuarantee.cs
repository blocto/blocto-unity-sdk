using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Http.Models.Apis
{
    public partial class CollectionGuarantee
    {
        [JsonProperty("collection_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Collection_id { get; set; }

        [JsonProperty("signer_ids")]
        [Required]
        [MinLength(1)]
        public ICollection<string> Signer_ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

        [JsonProperty("signature")]
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