using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class ResourceEventGuid
	{
		[JsonProperty(PropertyName = "id")]
		public ResourceEventId Id;
	}
}