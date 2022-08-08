using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flow.Net.Sdk.Core;
using Newtonsoft.Json;

namespace Flow.Net.SDK.Client.Unity.Models.Apis
{
    public partial class TransactionResult
    {
        [JsonProperty("block_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Block_id { get; set; }

        [JsonProperty("execution", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TransactionExecution Execution { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TransactionStatus Status { get; set; }

        [JsonProperty("status_code", Required = Required.Always)]
        public int Status_code { get; set; }

        /// <summary>Provided transaction error in case the transaction wasn't successful.</summary>
        [JsonProperty("error_message", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Error_message { get; set; }

        [JsonProperty("computation_used", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Computation_used { get; set; }

        [JsonProperty("events", Required = Required.Always)]
        [Required]
        public ICollection<Event> Events { get; set; } = new System.Collections.ObjectModel.Collection<Event>();

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