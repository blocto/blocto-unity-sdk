using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class TableItemRequest
	{
		[JsonProperty(PropertyName = "key_type")]
		public string KeyType;
		[JsonProperty(PropertyName = "value_type")]
		public string ValueType;
		[JsonProperty(PropertyName = "key")]
		public object Key;
	}
}