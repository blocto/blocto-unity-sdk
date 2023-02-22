using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class WriteSet
	{
		[JsonProperty(PropertyName = "type")]
		public string Type;
	}
}