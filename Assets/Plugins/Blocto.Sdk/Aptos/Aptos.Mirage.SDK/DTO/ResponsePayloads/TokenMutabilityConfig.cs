using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class TokenMutabilityConfig : CollectionMutabilityConfig
	{
		[JsonProperty(PropertyName = "properties")]
		public bool Properties;
		[JsonProperty(PropertyName = "royalty")]
		public bool Royalty;
	}
}