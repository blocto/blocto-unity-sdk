using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class Event
	{
		[JsonProperty(PropertyName = "guid")]
		public EventGuid Guid;
		[JsonProperty(PropertyName = "sequence_number")]
		public string SequenceNumber;
		[JsonProperty(PropertyName = "type")]
		public string Type;
		[JsonProperty(PropertyName = "data")]
		public JObject Data;
	}
}