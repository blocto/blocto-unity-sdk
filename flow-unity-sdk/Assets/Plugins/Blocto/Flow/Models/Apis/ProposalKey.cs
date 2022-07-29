using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Blocto.Flow.Client.Http.Models.Apis
{
    public partial class ProposalKey
    {
        [JsonProperty("address", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Address { get; set; }

        [JsonProperty("key_index", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Key_index { get; set; }

        [JsonProperty("sequence_number", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Sequence_number { get; set; }

        private IDictionary<string, object> _additionalProperties = new Dictionary<string, object>();

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}