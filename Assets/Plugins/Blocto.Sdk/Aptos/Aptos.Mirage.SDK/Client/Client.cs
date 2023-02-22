using System;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;
using Mirage.Aptos.SDK.DTO.ResponsePayloads;

namespace Mirage.Aptos.SDK
{
	/// <summary>
	/// Provides methods for retrieving data from Aptos node.
	/// </summary>
	/// <seealso href="https://fullnode.devnet.aptoslabs.com/v1/spec"/>
	public class Client
	{
		private const int DefaultMaxGasAmount = 200000;
		private const int DefaultTxnExpSecFromNow = 20;

		private readonly ClientServices _services;

		/// <summary>
		/// Build a client configured to connect to an Aptos node at the given URL.
		/// </summary>
		/// <param name="nodeUrl">URL of the Aptos Node API endpoint.</param>
		/// <param name="config">Additional configuration options for the generated Axios client.</param>
		public Client(string nodeUrl, OpenAPIConfig config = null)
		{
			if (config == null)
			{
				config = new OpenAPIConfig();
			}

			config.Base = nodeUrl;

			_services = new ClientServices(config);
		}

		public async Task PopulateRequestParams(
			SubmitTransaction transaction,
			OptionalTransactionArgs extraArgs = null
		)
		{
			var ledgerInfo = await _services.GeneralService.GetLedgerInfo();
			var account = await _services.AccountsService.GetAccount(transaction.Sender.Address);
			var gasUnitPrice = await GetGasUnitPrice(extraArgs);
			var expireTimestamp = GetExpireTimeStamp();

			transaction.SequenceNumber = account.SequenceNumber;
			transaction.MaxGasAmount = DefaultMaxGasAmount;
			transaction.GasUnitPrice = gasUnitPrice;
			transaction.ExpirationTimestampSecs = expireTimestamp;
			transaction.ChainID = ledgerInfo.ChainID;
		}

		public Task<PendingTransactionPayload> SubmitTransaction(SubmitTransactionRequest request)
		{
			return _services.TransactionsService.SubmitTransaction(request);
		}

		public Task<UserTransaction> SimulateTransaction(
			SubmitTransactionRequest requestBody,
			bool? estimateMaxGasAmount = null,
			bool? estimateGasUnitPrice = null,
			bool? estimatePrioritizedGasUnitPrice = null
		)
		{
			return _services.TransactionsService.SimulateTransaction(
				requestBody,
				estimateMaxGasAmount,
				estimateGasUnitPrice,
				estimatePrioritizedGasUnitPrice
			);
		}

		public Task<TypedTransaction> GetTransactionByHash(string hash)
		{
			return _services.TransactionsService.GetTransactionByHash(hash);
		}

		public Task<MoveResource> GetAccountResource(string account, string resourceType)
		{
			return _services.AccountsService.GetAccountResource(account, resourceType);
		}

		public Task<TReturn> GetTableItem<TReturn>(string tableHandle, TableItemRequest requestBody)
		{
			return _services.TableService.GetTableItem<TReturn>(tableHandle, requestBody);
		}

		public Task<VersionedEvent> GetEventsByCreationNumber(
			string address,
			ulong creationNumber,
			ulong? start = null,
			ulong? limit = null
		)
		{
			return _services.EventsService.GetEventsByCreationNumber(
				address,
				creationNumber,
				start,
				limit
			);
		}

		public Task<VersionedEvent> GetEventsByEventHandle(
			string address,
			string eventHandle,
			string fieldName,
			ulong? start = null,
			ulong? limit = null
		)
		{
			return _services.EventsService.GetEventsByEventHandle(
				address,
				eventHandle,
				fieldName,
				start,
				limit
			);
		}

		private async Task<ulong> GetGasUnitPrice(OptionalTransactionArgs extraArgs = null)
		{
			var gasUnitPrice = extraArgs?.GasUnitPrice;
			if (gasUnitPrice == null)
			{
				var gasEstimation = await _services.TransactionsService.EstimateGasPrice();
				gasUnitPrice = gasEstimation.GasEstimate;
			}

			return (ulong)gasUnitPrice;
		}

		private uint GetExpireTimeStamp()
		{
			return (uint)Math.Floor((double)(DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000 +
			                                 DefaultTxnExpSecFromNow));
		}
	}
}