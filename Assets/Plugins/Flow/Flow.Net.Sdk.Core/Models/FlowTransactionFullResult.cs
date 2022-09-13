using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Core.Models
{
    public class FlowTransactionFullResult : FlowTransactionResult
    {
        public FlowTransactionFullResult()
        {
            Events = new List<FlowEvent>();
        }

        [JsonProperty("events")]
        public IList<FlowEvent> Events { get; set; }
    }
}
