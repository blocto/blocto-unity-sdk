using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blocto.Sdk.Ethereum.Model.Rpc
{
    public class AccessList
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        [JsonProperty(PropertyName = "storageKeys")]
        public List<string> StorageKeys { get; set; }
    }
}