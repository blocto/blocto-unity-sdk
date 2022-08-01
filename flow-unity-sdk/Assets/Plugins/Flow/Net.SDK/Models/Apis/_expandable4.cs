using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Http.Models.Apis
{
    public partial class _expandable4
    {
        [JsonProperty("payload", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Payload { get; set; }

        [JsonProperty("execution_result", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Execution_result { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}