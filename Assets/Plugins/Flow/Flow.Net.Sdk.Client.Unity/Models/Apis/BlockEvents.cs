using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class BlockEvents
    {
        [JsonProperty("block_id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Block_id { get; set; }

        [JsonProperty("block_height", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Block_height { get; set; }

        [JsonProperty("block_timestamp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset Block_timestamp { get; set; }

        [JsonProperty("events", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Event> Events { get; set; }

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