using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Client.Unity.Models.Apis
{
    public partial class BlockHeader
    {
        [JsonProperty("id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Id { get; set; }

        [JsonProperty("parent_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Parent_id { get; set; }

        [JsonProperty("height", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Height { get; set; }

        [JsonProperty("timestamp", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("parent_voter_signature", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Parent_voter_signature { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}