using Blocto.Sdk.Evm.Utility;
using Newtonsoft.Json;

namespace Blocto.Sdk.Evm.Model
{
    public class EvmTransaction
    {
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("to")]
        public string To { get; set; }
        
        [JsonProperty("value")]
        public string HexValue { get; private set; }
        
        [JsonIgnore]
        public decimal Value
        {
            set {
                var transactionValue = EthConvert.ToWei(value);
                var valueHex = transactionValue.ToString("X");
                HexValue = $"0x{valueHex}";
            }
        }
        
        [JsonProperty("gas")]
        public string Gas { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }
        
        [JsonProperty("maxPriorityFeePerGas")]
        public string MaxPriorityFeePerGas { get; set; }
        
        [JsonProperty("maxFeePerGas")]
        public string MaxFeePerGas { get; set; }
    }
}