using System.Numerics;
using System.Threading.Tasks;
using Mirage.Aptos.Constants;
using Mirage.Aptos.SDK.Constants;
using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK
{
	public class CoinClient: SpecificClient
	{
		private const string AptosCoinType = "0x1::aptos_coin::AptosCoin";

		/// <summary>
		/// Creates new CoinClient instance
		/// </summary>
		/// <param name="client"><see cref="Client"/> instance.</param>
		public CoinClient(Client client) : base(client, ABIs.GetCoinABIs())
		{
		}

		/// <summary>
		/// Generate, sign, and submit a transaction to the Aptos blockchain API to
		/// transfer AptosCoin from one account to another.
		/// </summary>
		/// <param name="from">Account sending the coins.</param>
		/// <param name="to">Account to receive the coins.</param>
		/// <param name="amount">Number of coins to transfer.</param>
		/// <returns>The transaction submitted to the API.</returns>
		public async Task<PendingTransactionPayload> Transfer(Account from, Account to, ulong amount)
		{
			var request = await CreateTransaction(from, to, amount);
			var receipt = await _client.SubmitTransaction(request);
			return receipt;
		}
		
		
		public async Task<UserTransaction> SimulateTransfer(Account from, Account to, ulong amount)
		{
			var request = await CreateTransaction(from, to, amount);
			var receipt = await _client.SimulateTransaction(request);
			return receipt;
		}

		/// <summary>
		/// Get amount of AptosCoin on given account.
		/// </summary>
		/// <param name="account">Account that you want to check the balance of.</param>
		/// <returns>Task with the balance amount.</returns>
		public async Task<BigInteger> GetBalance(Account account)
		{
			var resource = await _client.GetAccountResource(account.Address, ResourcesTypes.AptosCoin);

			var data = resource.Data.ToObject<CoinStoreType>();

			return BigInteger.Parse(data.Coin.Value);
		}
		
		private async Task<SubmitTransactionRequest> CreateTransaction(Account from, Account to, ulong amount)
		{
			var payload = GetPayload(to, amount);
			var transaction = await PrepareTransaction(from, payload);

			var raw = transaction.GetRaw();
			var signature = _signatureBuilder.GetSignature(from, raw);
			var request = transaction.GetRequest(payload, signature);

			return request;
		}

		private EntryFunctionPayload GetPayload(Account to, ulong amount)
		{
			return new EntryFunctionPayload
			{
				Type = TransactionPayloadTypes.EntryFunction,
				Function = FunctionTypes.Transfer,
				TypeArguments = new string[] { AptosCoinType },
				Arguments = new string[] { to.Address, amount.ToString() }
			};
		}
	}
}