using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class Event
    {
        [JsonProperty("type", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Type { get; set; }

        [JsonProperty("transaction_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Transaction_id { get; set; }

        [JsonProperty("transaction_index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Transaction_index { get; set; }

        [JsonProperty("event_index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Event_index { get; set; }

        [JsonProperty("payload", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public byte[] Payload { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}