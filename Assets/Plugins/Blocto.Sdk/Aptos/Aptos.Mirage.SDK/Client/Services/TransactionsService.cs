using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirage.Aptos.SDK.DTO;
using Newtonsoft.Json;

namespace Mirage.Aptos.SDK.Services
{
	public class TransactionsService : BaseService
	{
		private const string EstimateGasPriceRoute = "/estimate_gas_price";
		private const string SubmitTransactionRoute = "/transactions";
		private const string SimulateTransactionRoute = "/transactions/simulate";
		private const string GetTransactionByHashRoute = @"/transactions/by_hash/{0}";
		private const string JsonWrapperForTransaction = @"{{""transaction"":{0}}}";
		private const string EstimateMaxGasAmountField = "estimate_max_gas_amount";
		private const string EstimateGasUnitPriceField = "estimate_gas_unit_price";
		private const string EstimatePrioritizedGasUnitPriceField = "estimate_prioritized_gas_unit_price";

		public TransactionsService(OpenAPIConfig config) : base(config)
		{
		}

		public Task<PendingTransactionPayload> SubmitTransaction(SubmitTransactionRequest requestBody)
		{
			return WebHelper.SendPostRequest<SubmitTransactionRequest, PendingTransactionPayload>(URL + SubmitTransactionRoute,
				requestBody);
		}

		public async Task<TypedTransaction> GetTransactionByHash(string hash)
		{
			var url = URL + string.Format(GetTransactionByHashRoute, hash);
			var wrapper = await WebHelper.SendGetRequest<WrappedTransaction>(url, wrapper: JsonWrapperForTransaction);
			return wrapper.Transaction;
		}

		public Task<GasEstimation> EstimateGasPrice()
		{
			return WebHelper.SendGetRequest<GasEstimation>(URL + EstimateGasPriceRoute);
		}

		public Task<UserTransaction> SimulateTransaction(
			SubmitTransactionRequest requestBody,
			bool? estimateMaxGasAmount,
			bool? estimateGasUnitPrice,
			bool? estimatePrioritizedGasUnitPrice
		)
		{
			var query = new Dictionary<string, string>();

			if (estimateMaxGasAmount != null)
			{
				query.Add(EstimateMaxGasAmountField, estimateMaxGasAmount.ToString());
			}
			
			if (estimateGasUnitPrice != null)
			{
				query.Add(EstimateGasUnitPriceField, estimateGasUnitPrice.ToString());
			}
			
			if (estimatePrioritizedGasUnitPrice != null)
			{
				query.Add(EstimatePrioritizedGasUnitPriceField, estimatePrioritizedGasUnitPrice.ToString());
			}

			return WebHelper.SendPostRequest<SubmitTransactionRequest, UserTransaction>(
				URL + SimulateTransactionRoute, requestBody, query: query);
		}
	}
}