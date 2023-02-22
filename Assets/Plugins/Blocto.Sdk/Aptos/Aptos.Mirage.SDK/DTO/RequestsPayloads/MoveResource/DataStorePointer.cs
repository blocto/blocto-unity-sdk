using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mirage.Aptos.SDK.DTO
{
	public class DataStorePointer
	{
		[JsonProperty(PropertyName = "handle")]
		public string Handle;
	}
}