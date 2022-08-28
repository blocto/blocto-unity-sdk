using Newtonsoft.Json;

namespace Flow.FCL.Models
{
    public class Collection
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}