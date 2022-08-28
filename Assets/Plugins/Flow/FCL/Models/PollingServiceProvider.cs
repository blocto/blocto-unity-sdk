using System;
using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public partial class PollingServiceProvider
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}