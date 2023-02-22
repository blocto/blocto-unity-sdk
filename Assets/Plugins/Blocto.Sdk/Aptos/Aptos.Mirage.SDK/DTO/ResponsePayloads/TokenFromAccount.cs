using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class TokenFromAccount
	{
		[JsonProperty(PropertyName = "id")]
		public TokenId Id;
		[JsonProperty(PropertyName = "amount")]
		public string Amount;
	}
}