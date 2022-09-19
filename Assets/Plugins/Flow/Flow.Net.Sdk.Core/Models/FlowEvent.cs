using Flow.Net.Sdk.Core.Cadence;
using Newtonsoft.Json;

namespace Flow.Net.Sdk.Core.Models
{

    [JsonConverter(typeof(FlowEventConverter))]
    public class FlowEvent
    {
        public uint EventIndex { get; set; }
        public ICadence Payload { get; set; }
        public string TransactionId { get; set; }
        public uint TransactionIndex { get; set; }
        public string Type { get; set; }
    }
}
