using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class BlockPayload
    {
        [JsonProperty("collection_guarantees", Required = Required.Always)]
        [Required]
        public ICollection<CollectionGuarantee> Collection_guarantees { get; set; } = new System.Collections.ObjectModel.Collection<CollectionGuarantee>();

        [JsonProperty("block_seals", Required = Required.Always)]
        [Required]
        public ICollection<BlockSeal> Block_seals { get; set; } = new System.Collections.ObjectModel.Collection<BlockSeal>();

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}