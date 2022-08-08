using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class Account
    {
        [JsonProperty("address", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Address { get; set; }

        /// <summary>Flow balance of the account.</summary>
        [JsonProperty("balance", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Balance { get; set; }

        [JsonProperty("keys", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [MinLength(1)]
        public ICollection<AccountPublicKey> Keys { get; set; }

        [JsonProperty("contracts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, byte[]> Contracts { get; set; }

        [JsonProperty("_expandable", Required = Required.Always)]
        [Required]
        public _expandable _expandable { get; set; } = new _expandable();

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