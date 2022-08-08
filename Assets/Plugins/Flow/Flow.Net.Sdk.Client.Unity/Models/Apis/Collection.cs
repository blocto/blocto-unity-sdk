using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class Collection
    {
        [JsonProperty("id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Id { get; set; }

        [JsonProperty("transactions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Transaction> Transactions { get; set; }

        [JsonProperty("_expandable", Required = Required.Always)]
        [Required]
        public _expandable2 _expandable { get; set; } = new _expandable2();

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