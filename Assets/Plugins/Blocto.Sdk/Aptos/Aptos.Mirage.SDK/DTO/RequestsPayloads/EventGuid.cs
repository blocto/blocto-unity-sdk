using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class EventGuid
	{
		[JsonProperty(PropertyName = "creation_number")]
		public string CreationNumber;
		[JsonProperty(PropertyName = "account_address")]
		public string AccountAddress;
	}
}