using System.Collections.Generic;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;

namespace Mirage.Aptos.SDK
{
	/// <summary>
	/// Class for requesting tokens from faucet.
	/// </summary>
	public class FaucetClient
	{
		private const string FundAccountRoute = "/mint";
		
		private string _url;
		private Client _client;
		
		/// <summary>
		/// Establishes a connection to Aptos node.
		/// </summary>
		/// <param name="faucetUrl">A faucet url.</param>
		/// <param name="client">The Aptos client.</param>
		public FaucetClient(string faucetUrl, Client client)
		{
			_url = faucetUrl;
			_client = client;
		}

		/// <summary>
		/// This creates an account if it does not exist and mints the specified amount of
		/// coins into that account.
		/// </summary>
		/// <param name="account">Aptos account witch mints tokens.</param>
		/// <param name="amount">Amount of tokens to mint.</param>
		/// <returns>Task with submitted transactions.</returns>
		public async Task<TypedTransaction[]> FundAccount(Account account, uint amount)
		{
			var query = new Dictionary<string, string>
			{
				{ "address", account.Address },
				{ "amount", amount.ToString() }
			};
			var txnHashes = await WebHelper.SendPostRequest<string[]>(_url + FundAccountRoute, query: query);

			var awaitedAnswers = await Task.WhenAll(CreateAwaiters(txnHashes));

			return awaitedAnswers;
		}

		private IEnumerable<Task<TypedTransaction>> CreateAwaiters(string[] hashes)
		{
			foreach (var hash in hashes)
			{
				var awaiter = new TransactionAwaiter(_client);
				yield return awaiter.WaitForTransactionWithResult(hash);
			}
		}
	}
}