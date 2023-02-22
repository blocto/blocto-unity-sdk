using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveStructField
	{
		[JsonProperty(PropertyName = "name")]
		public string Name;
		[JsonProperty(PropertyName = "type")]
		public string Type;
	}
}