using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class ResourceEvent
	{
		[JsonProperty(PropertyName = "counter")]
		public string Counter;
		[JsonProperty(PropertyName = "guid")]
		public ResourceEventGuid Guid;
	}
}