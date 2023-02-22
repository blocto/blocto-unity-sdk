using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class ResourceEventId
	{
		[JsonProperty(PropertyName = "addr")]
		public string Addr;
		[JsonProperty(PropertyName = "creation_num")]
		public string CreationNum;
	}
}