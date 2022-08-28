using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class Event
    {
        public Event()
        {
            BlockIds = new List<string>();    
        }
        
        [JsonProperty("eventType")]
        public string EventType { get; set; }
        
        [JsonProperty("start")]
        public string Start { get; set; }
        
        [JsonProperty("end")]
        public string End { get; set; }
        
        [JsonProperty("blockIds")]
        public List<string> BlockIds { get; set; }
    }
}