using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class Chunk
    {
        [JsonProperty("block_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Block_id { get; set; }

        [JsonProperty("collection_index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Collection_index { get; set; }

        [JsonProperty("start_state", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Start_state { get; set; }

        [JsonProperty("end_state", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] End_state { get; set; }

        [JsonProperty("event_collection", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Event_collection { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Index { get; set; }

        [JsonProperty("number_of_transactions", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Number_of_transactions { get; set; }

        [JsonProperty("total_computation_used", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Total_computation_used { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}