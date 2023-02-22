using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class TokenId
	{
		[JsonProperty(PropertyName = "token_data_id")]
		public TokenDataId TokenDataId;
		[JsonProperty(PropertyName = "property_version")]
		public string PropertyVersion;
	}
}