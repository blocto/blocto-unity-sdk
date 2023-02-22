using System;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.DTO
{
	public class EntryFunctionPayload : TransactionPayload
	{
		[JsonProperty(PropertyName = "function")]
		public string Function;
		[JsonProperty(PropertyName = "type_arguments")]
		public string[] TypeArguments;
		[JsonProperty(PropertyName = "arguments")]
		public object[] Arguments;
	}
}