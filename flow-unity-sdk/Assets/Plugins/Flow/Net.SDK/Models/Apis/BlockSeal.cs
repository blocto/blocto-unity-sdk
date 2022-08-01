using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Http.Models.Apis
{
    public partial class BlockSeal
    {
        [JsonProperty("block_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Block_id { get; set; }

        [JsonProperty("result_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Result_id { get; set; }

        [JsonProperty("final_state", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Final_state { get; set; }

        [JsonProperty("aggregated_approval_signatures", Required = Required.Always)]
        [Required]
        [MinLength(1)]
        public ICollection<AggregatedSignature> Aggregated_approval_signatures { get; set; } = new System.Collections.ObjectModel.Collection<AggregatedSignature>();

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}