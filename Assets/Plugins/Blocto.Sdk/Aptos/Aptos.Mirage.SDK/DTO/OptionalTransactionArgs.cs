namespace Mirage.Aptos.SDK.DTO
{
	public class OptionalTransactionArgs
	{
		public ulong? MaxGasAmount { get; set; }
		public ulong? GasUnitPrice { get; set; }
		public ulong? ExpireTimestamp { get; set; }
	}
}