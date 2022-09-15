using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flow.Net.Sdk.Core.Models
{
    public class FlowTransactionResult
    {
        public FlowTransactionResult()
        {
        }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }
        
        [JsonProperty("execution")]
        public TransactionExecution Execution { get; set;}
        
        // [JsonProperty("status")]
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public TransactionStatus Status { get; set; }
        
        [JsonProperty("error_message")]
        /// <summary>Provided transaction error in case the transaction wasn't successful.</summary>
        public string ErrorMessage { get; set; }        

        [JsonProperty("events")]
        public IList<FlowEvent> Events { get; set; }
        
        [JsonProperty("status_code")]
        public uint StatusCode { get; set; }
    }
}
