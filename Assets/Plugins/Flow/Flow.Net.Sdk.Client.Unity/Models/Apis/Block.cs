using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class Block
    {
        [JsonProperty("header", Required = Required.Always)]
        [Required]
        public BlockHeader Header { get; set; } = new BlockHeader();

        [JsonProperty("payload", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public BlockPayload Payload { get; set; }

        [JsonProperty("execution_result", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ExecutionResult Execution_result { get; set; }

        [JsonProperty("_expandable", Required = Required.Always)]
        [Required]
        public _expandable4 _expandable { get; set; } = new _expandable4();

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