using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Http.Models.Apis
{
    public partial class Error
    {
        [JsonProperty("code", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int Code { get; set; }

        [JsonProperty("message", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        } 
    }
}