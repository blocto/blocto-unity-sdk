using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO.ResponsePayloads
{
	public class VersionedEvent : Event
	{
		[JsonProperty(PropertyName = "version")]
		public string Version;
	}
}