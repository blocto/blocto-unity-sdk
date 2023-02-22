using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveScriptBytecode
	{
		[JsonProperty(PropertyName = "bytecode")]
		public string Bytecode;
		[JsonProperty(PropertyName = "abi")]
		public MoveFunction ABI;
	}
}