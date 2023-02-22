using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class CoinStoreType
	{
		[JsonProperty(PropertyName = "coin")]
		public CoinValue Coin;
	}
}