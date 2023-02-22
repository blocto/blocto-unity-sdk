using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class ScriptWriteSet : WriteSet
	{
		[JsonProperty(PropertyName = "execute_as")]
		public string ExecuteAs;
		[JsonProperty(PropertyName = "script")]
		public ScriptPayload Script;
	}
}