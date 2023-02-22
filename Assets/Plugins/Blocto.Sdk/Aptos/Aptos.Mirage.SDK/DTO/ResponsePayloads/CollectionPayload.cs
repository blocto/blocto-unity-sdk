using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class CollectionPayload
	{
		[JsonProperty(PropertyName = "description")]
		public string Description;
		[JsonProperty(PropertyName = "maximum")]
		public string Maximum;
		[JsonProperty(PropertyName = "mutability_config")]
		public CollectionMutabilityConfig MutabilityConfig;
		[JsonProperty(PropertyName = "name")]
		public string Name;
		[JsonProperty(PropertyName = "supply")]
		public string Supply;
		[JsonProperty(PropertyName = "uri")]
		public string Uri;
	}
}