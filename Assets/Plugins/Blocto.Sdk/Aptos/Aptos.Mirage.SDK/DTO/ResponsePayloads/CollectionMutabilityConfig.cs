using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class CollectionMutabilityConfig
	{
		[JsonProperty(PropertyName = "description")]
		public bool Description;
		[JsonProperty(PropertyName = "maximum")]
		public bool Maximum;
		[JsonProperty(PropertyName = "uri")]
		public bool Uri;
	}
}