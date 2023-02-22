using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class DefaultProperties
	{
		// TODO Need to change type based on the payload
		[JsonProperty(PropertyName = "map")]
		public JObject Map;
	}
}