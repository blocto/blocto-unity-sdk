using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class TokenPayload
	{
		[JsonProperty(PropertyName = "default_properties")]
		public DefaultProperties DefaultProperties;
		[JsonProperty(PropertyName = "description")]
		public string Description;
		[JsonProperty(PropertyName = "largest_property_version")]
		public string LargestPropertyVersion;
		[JsonProperty(PropertyName = "maximum")]
		public string Maximum;
		[JsonProperty(PropertyName = "mutability_config")]
		public TokenMutabilityConfig MutabilityConfig;
		[JsonProperty(PropertyName = "name")]
		public string Name;
		[JsonProperty(PropertyName = "royalty")]
		public RoyaltyConfig Royalty;
		[JsonProperty(PropertyName = "supply")]
		public string Supply;
		[JsonProperty(PropertyName = "uri")]
		public string Uri;
	}
}