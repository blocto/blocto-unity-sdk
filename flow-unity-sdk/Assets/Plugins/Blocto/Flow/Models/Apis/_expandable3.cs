using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Flow.Client.Http.Models.Apis
{
    public partial class _expandable3
    {
        [JsonProperty("result", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Result { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}