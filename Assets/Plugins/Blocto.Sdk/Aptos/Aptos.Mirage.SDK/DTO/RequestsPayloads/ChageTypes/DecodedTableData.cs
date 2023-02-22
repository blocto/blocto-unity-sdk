using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnkrSDK.Runtime.Aptos.DTO.RequestsPayloads.ChageTypes
{
	public class DecodedTableData
	{
		[JsonProperty(PropertyName = "key")]
		// TODO Implement value deserialization
		public JObject Key;
		[JsonProperty(PropertyName = "key_type")]
		public string KeyType;
		[JsonProperty(PropertyName = "value")]
		// TODO Implement value deserialization
		public JObject Value;
		[JsonProperty(PropertyName = "value_type")]
		public string ValueType;
	}
}