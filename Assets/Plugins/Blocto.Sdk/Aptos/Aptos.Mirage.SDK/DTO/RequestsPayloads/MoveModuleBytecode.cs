using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class MoveModuleBytecode
	{
		[JsonProperty(PropertyName = "bytecode")]
		public string Bytecode;
		[JsonProperty(PropertyName = "abi")]
		public MoveModule ABI;
	}
}