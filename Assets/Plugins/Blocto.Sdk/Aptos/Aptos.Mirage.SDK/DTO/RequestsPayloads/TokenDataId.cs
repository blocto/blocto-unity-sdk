using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class TokenDataId
	{
		[JsonProperty(PropertyName = "creator")]
		public string Creator;
		[JsonProperty(PropertyName = "collection")]
		public string Collection;
		[JsonProperty(PropertyName = "name")]
		public object Name;
	}
}