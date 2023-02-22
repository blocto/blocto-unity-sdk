using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class ScriptPayload : TransactionPayload
	{
		[JsonProperty(PropertyName = "code")]
		public MoveScriptBytecode Code;
		[JsonProperty(PropertyName = "type_arguments")]
		public string[] TypeArguments;
		[JsonProperty(PropertyName = "arguments")]
		public object[] Arguments;
	}
}