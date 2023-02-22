using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.Imlementation.ABI;

namespace Mirage.Aptos.SDK
{
	public abstract class SpecificClient
	{
		protected Client _client;
		protected TransactionBuilderABI _transactionBuilder;
		protected Ed25519SignatureBuilder _signatureBuilder;
		
		protected SpecificClient(Client client, string[] ABIs)
		{
			_client = client;
			_transactionBuilder = new TransactionBuilderABI(ABIs);
			_signatureBuilder = new Ed25519SignatureBuilder();
		}
		
		protected async Task<SubmitTransaction> PrepareTransaction(
			Account from,
			EntryFunctionPayload payload,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var transaction = new SubmitTransaction
			{
				Sender = from,
				Payload = _transactionBuilder.BuildTransactionPayload(
					payload.Function,
					payload.TypeArguments,
					payload.Arguments
				)
			};

			await _client.PopulateRequestParams(transaction, extraArgs);

			return transaction;
		}
	}
}