using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Flow.Client.Http.Models.Apis
{
    public partial class Links
    {
        [JsonProperty("_self", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string _self { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        } 
    }
}