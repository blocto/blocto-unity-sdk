using Mirage.Aptos.SDK.Converters;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class DirectWriteSet : WriteSet
	{
		[JsonProperty(PropertyName = "changes")]
		[JsonConverter(typeof(WriteSetChangeArrayConverter))]
		public WriteSetChange[] Changes;
		[JsonProperty(PropertyName = "events")]
		public Event[] Events;
	}
}