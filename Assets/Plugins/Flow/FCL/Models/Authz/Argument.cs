using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.FCL.Models.Authz
{
    public class Argument
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        
        [JsonProperty("tempId")]
        public string TempId { get; set; }
        
        [JsonProperty("value")]
        public object Value { get; set; }
        
        [JsonProperty("asArgument")]
        public BaseArgument AsBaseArgument { get; set; }
        
        [JsonProperty("xform")]
        public JObject XForm { get; set; }
    }
}