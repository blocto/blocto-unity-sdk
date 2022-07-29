using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Blocto.Flow.Client.Http.Models.Apis
{
    public partial class ExecutionResult
    {
        [JsonProperty("id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Id { get; set; }

        [JsonProperty("block_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Block_id { get; set; }

        [JsonProperty("events", Required = Required.Always)]
        [Required]
        public ICollection<Event> Events { get; set; } = new System.Collections.ObjectModel.Collection<Event>();

        [JsonProperty("chunks", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Chunk> Chunks { get; set; }

        [JsonProperty("previous_result_id", Required = Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Previous_result_id { get; set; }

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