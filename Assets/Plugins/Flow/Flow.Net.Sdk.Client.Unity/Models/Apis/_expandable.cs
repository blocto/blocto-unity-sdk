using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class _expandable
    {
        [JsonProperty("keys", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Keys { get; set; }

        [JsonProperty("contracts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Contracts { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}