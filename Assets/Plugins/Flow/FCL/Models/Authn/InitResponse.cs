using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flow.FCL.Models.Authn
{
    public partial class InitResponse
    {
        [JsonProperty("f_type")]
        public string FType { get; set; }

        [JsonProperty("f_vsn")]
        public string FVsn { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("updates")]
        public Updates Updates { get; set; }

        [JsonProperty("local")]
        public Local Local { get; set; }
    }
}