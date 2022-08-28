using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class XForm
    {
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}