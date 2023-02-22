using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class WriteSetChange
	{
		[JsonProperty(PropertyName = "type")]
		public string Type;
	}
}