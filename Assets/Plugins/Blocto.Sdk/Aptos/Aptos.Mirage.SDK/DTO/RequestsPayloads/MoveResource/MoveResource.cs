using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveResource
	{
		[JsonProperty(PropertyName = "type")]
		public string Type;
		[JsonProperty(PropertyName = "data")]
		// TODO Implement converter for different data types https://fullnode.devnet.aptoslabs.com/v1/spec#/schemas/MoveStructValue
		public JObject Data;
	}
}