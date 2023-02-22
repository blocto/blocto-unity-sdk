using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class CoinValue
	{
		[JsonProperty(PropertyName = "value")]
		public string Value;
	}
}