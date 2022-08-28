using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class BaseArgument
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }
}