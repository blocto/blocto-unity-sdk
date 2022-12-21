using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class ScriptBody
    {
        /// <summary>Base64 encoded content of the Cadence script.</summary>
        [JsonProperty("script", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Script { get; set; }

        /// <summary>An list of arguments each encoded as Base64 passed in the [JSON-Cadence interchange format](https://docs.onflow.org/cadence/json-cadence-spec/).</summary>
        [JsonProperty("arguments", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<byte[]> Arguments { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}