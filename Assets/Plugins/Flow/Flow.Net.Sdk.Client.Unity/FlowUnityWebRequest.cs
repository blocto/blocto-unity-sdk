
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Exceptions;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.Net.SDK.Client.Unity.Unity
{
    public class FlowUnityWebRequest : IFlowClient
    {
        private readonly FlowApiV1 _flowApiV1;

        /// <summary>
        /// A HTTP UnityWebRequest for the Flow v1 API
        /// </summary>
        /// <param name="serverUrl"></param>
        public FlowUnityWebRequest(GameObject gameObject, string serverUrl)
        {
            _flowApiV1 = gameObject.AddComponent<FlowApiV1>();
            _flowApiV1.BaseUrl = serverUrl;
        }

        public Task PingAsync() => throw new NotImplementedException();

        public Task<FlowBlockHeader> GetLatestBlockHeaderAsync(bool isSealed = true) => GetLatestBlockHeaderAsync(CancellationToken.None, isSealed);

        /// <summary>
        /// Gets the latest sealed or unsealed block header.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="isSealed"></param>
        /// <returns><see cref="FlowBlockHeader"/></returns>
        /// <exception cref="FlowException"></exception>
        private async Task<FlowBlockHeader> GetLatestBlockHeaderAsync(CancellationToken cancellationToken, bool isSealed = true)
        {
            try
            {
                var response = _flowApiV1.BlocksAll(new List<string> { isSealed ? "sealed" : "final" }, null, null, null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault()?.Header);
            }
            catch (Exception ex)
            {
                throw new FlowException("Get latest block header error", ex);
            }
        }
        
        public Task<FlowBlockHeader> GetBlockHeaderByIdAsync(string blockId) => GetBlockHeaderByIdAsync(blockId, CancellationToken.None);

        /// <summary>
        /// Gets a block header by Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowBlockHeader"/></returns>
        /// <exception cref="FlowException"></exception>
        private async Task<FlowBlockHeader> GetBlockHeaderByIdAsync(string blockId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Blocks(new List<string> { blockId }, null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault()?.Header);
            }
            catch (Exception ex)
            {
                throw new FlowException("Get block header by Id error", ex);
            }
        }
        
        public Task<FlowBlockHeader> GetBlockHeaderByHeightAsync(ulong height) => GetBlockHeaderByHeightAsync(height, CancellationToken.None);

        /// <summary>
        /// Gets a block header by height.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowBlockHeader"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowBlockHeader> GetBlockHeaderByHeightAsync(ulong height, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.BlocksAll(new List<string> { height.ToString() }, null, null, null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault()?.Header);
            }
            catch (Exception ex)
            {
                throw new FlowException("Get block header by height error", ex);
            }
        }
        
        public Task<FlowBlock> GetLatestBlockAsync(bool isSealed = true) => GetLatestBlockAsync(CancellationToken.None, isSealed);

        /// <summary>
        /// Gets the full payload of the latest sealed or unsealed block.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="isSealed"></param>
        /// <returns><see cref="FlowBlock"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowBlock> GetLatestBlockAsync(CancellationToken cancellationToken, bool isSealed = true)
        {
            try
            {
                var response = _flowApiV1.BlocksAll(new List<string> { isSealed ? "sealed" : "final" }, null, null, null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get latest block error", ex);
            }
        }
        
        public Task<FlowBlock> GetBlockByIdAsync(string blockId) => GetBlockByIdAsync(blockId, CancellationToken.None);
        
        /// <summary>
        /// Gets a full block by Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowBlock"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowBlock> GetBlockByIdAsync(string blockId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Blocks(new List<string> { blockId }, new List<string> { "payload" }, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get block by Id error", ex);
            }
        }

        public Task<FlowBlock> GetBlockByHeightAsync(ulong height) => GetBlockByHeightAsync(height, CancellationToken.None);

        /// <summary>
        /// Gets a full block by height.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowBlock"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowBlock> GetBlockByHeightAsync(ulong height, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.BlocksAll(new List<string> { height.ToString() }, null, null, new List<string> { "payload" }, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlock().FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get block by height error", ex);
            }
        }
        
        public Task<FlowCollection> GetCollectionAsync(string collectionId) => GetCollectionAsync(collectionId, CancellationToken.None);

        /// <summary>
        /// Gets a collection by Id.
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowCollection"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowCollection> GetCollectionAsync(string collectionId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Collections(collectionId, new List<string> { "transactions" }, cancellationToken);
                return await Task.FromResult(response.ToFlowCollection());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get collection error", ex);
            }
        }
        
        public Task<FlowTransactionId> SendTransactionAsync(FlowTransaction flowTransaction) => SendTransactionAsync(flowTransaction, CancellationToken.None);
        
        /// <summary>
        /// Submits a transaction to the network.
        /// </summary>
        /// <param name="flowTransaction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowTransactionId"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowTransactionId> SendTransactionAsync(FlowTransaction flowTransaction, CancellationToken cancellationToken)
        {
            try
            {
                var converted = flowTransaction.FromFlowTransaction();
                var response = _flowApiV1.SendTransaction(converted, cancellationToken);
                return await Task.FromResult(response.ToFlowTransactionId());
            }
            catch (Exception ex)
            {
                throw new FlowException("Send transaction error", ex);
            }
        }

        public Task<FlowTransactionResponse> GetTransactionAsync(string transactionId) => GetTransactionAsync(transactionId, CancellationToken.None);
        
        /// <summary>
        /// Gets a transaction by Id.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowTransactionResponse"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowTransactionResponse> GetTransactionAsync(string transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Transactions(transactionId, null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowTransactionResponse());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get transaction error", ex);
            }
        }

        public Task<FlowTransactionResult> GetTransactionResultAsync(string transactionId) => GetTransactionResultAsync(transactionId, CancellationToken.None);
        
        /// <summary>
        /// Gets the result of a transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowTransactionResult"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowTransactionResult> GetTransactionResultAsync(string transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.TransactionsResult(transactionId);
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                throw new FlowException("Get transaction result error", ex);
            }
        }

        public Task<FlowAccount> GetAccountAtLatestBlockAsync(string address) => GetAccountAtLatestBlockAsync(address, CancellationToken.None);

        /// <summary>
        /// Gets an account by address at the latest sealed block.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowAccount"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowAccount> GetAccountAtLatestBlockAsync(string address, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Accounts(address, "sealed", new List<string> { "contracts", "keys" }, cancellationToken);
                return await Task.FromResult(response.ToFlowAccount());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get account at latest block error", ex);
            }
        }
        
        public Task<FlowAccount> GetAccountAtBlockHeightAsync(string address, ulong blockHeight) => GetAccountAtBlockHeightAsync(address, blockHeight, CancellationToken.None);

        /// <summary>
        /// Gets an account by address at the given block height.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="blockHeight"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowAccount"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowAccount> GetAccountAtBlockHeightAsync(string address, ulong blockHeight, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Accounts(address, blockHeight.ToString(), new List<string> { "keys", "contracts" }, cancellationToken);
                return await Task.FromResult(response.ToFlowAccount());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get account at block height error", ex);
            }
        }
        
        public Task<ICadence> ExecuteScriptAtLatestBlockAsync(FlowScript flowScript) => ExecuteScriptAtLatestBlockAsync(flowScript, CancellationToken.None);
        
        /// <summary>
        /// Executes a read-only Cadence script against the latest sealed execution state.
        /// </summary>
        /// <param name="flowScript"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="ICadence"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<ICadence> ExecuteScriptAtLatestBlockAsync(FlowScript flowScript, CancellationToken cancellationToken)
        {
            try
            {
                var converted = flowScript.FromFlowScript();
                var response = _flowApiV1.Scripts(null, null, converted, cancellationToken);
                return await Task.FromResult(response.Value.Decode());
            }
            catch (Exception ex)
            {
                throw new FlowException("Execute script at latest block error", ex);
            }
        }

        public Task<ICadence> ExecuteScriptAtBlockIdAsync(FlowScript flowScript, string blockId) => ExecuteScriptAtBlockIdAsync(flowScript, blockId, CancellationToken.None);

        /// <summary>
        /// Executes a ready-only Cadence script against the execution state at the block with the given Id.
        /// </summary>
        /// <param name="flowScript"></param>
        /// <param name="blockId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="ICadence"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<ICadence> ExecuteScriptAtBlockIdAsync(FlowScript flowScript, string blockId, CancellationToken cancellationToken)
        {
            try
            {
                var converted = flowScript.FromFlowScript();
                var response = _flowApiV1.Scripts(blockId, null, converted, cancellationToken);
                return await Task.FromResult(response.Value.Decode());
            }
            catch (Exception ex)
            {
                throw new FlowException("Execute script at block Id error", ex);
            }
        }
        
        public Task<ICadence> ExecuteScriptAtBlockHeightAsync(FlowScript flowScript, ulong blockHeight) => ExecuteScriptAtBlockHeightAsync(flowScript, blockHeight, CancellationToken.None);

        /// <summary>
        /// Executes a ready-only Cadence script against the execution state at the given block height.
        /// </summary>
        /// <param name="flowScript"></param>
        /// <param name="blockHeight"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="ICadence"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<ICadence> ExecuteScriptAtBlockHeightAsync(FlowScript flowScript, ulong blockHeight, CancellationToken cancellationToken)
        {
            try
            {
                var converted = flowScript.FromFlowScript();
                var response = _flowApiV1.Scripts(null, blockHeight.ToString(), converted, cancellationToken);
                return await Task.FromResult(response.Value.Decode());
            }
            catch (Exception ex)
            {
                throw new FlowException("Execute script at block height error", ex);
            }
        }
        
        public Task<IEnumerable<FlowBlockEvent>> GetEventsForHeightRangeAsync(string eventType, ulong startHeight, ulong endHeight) => GetEventsForHeightRangeAsync(eventType, startHeight, endHeight, CancellationToken.None);

        /// <summary>
        /// Retrieves events for all sealed blocks between the start and end block heights (inclusive) with the given type.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="startHeight"></param>
        /// <param name="endHeight"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="FlowBlockEvent"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<IEnumerable<FlowBlockEvent>> GetEventsForHeightRangeAsync(string eventType, ulong startHeight, ulong endHeight, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Events(eventType, startHeight.ToString(), endHeight.ToString(), null, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlockEvent());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get events for height range error", ex);
            }
        }
        
        public Task<IEnumerable<FlowBlockEvent>> GetEventsForBlockIdsAsync(string eventType, IEnumerable<string> blockIds) => GetEventsForBlockIdsAsync(eventType, blockIds, CancellationToken.None);
        
        /// <summary>
        /// Retrieves events with the given type from the specified block Ids.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="blockIds"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="FlowBlockEvent"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<IEnumerable<FlowBlockEvent>> GetEventsForBlockIdsAsync(string eventType, IEnumerable<string> blockIds, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.Events(eventType, null, null, blockIds, null, cancellationToken);
                return await Task.FromResult(response.ToFlowBlockEvent());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get events for block Ids error", ex);
            }
        }
        
        public Task<FlowProtocolStateSnapshot> GetLatestProtocolStateSnapshotAsync() => throw new NotImplementedException("get latest protocol snapshot is currently not supported for HTTP API, if you require this functionality please use gRPC.");

        public Task<FlowExecutionResult> GetExecutionResultForBlockIdAsync(string blockId) => GetExecutionResultForBlockIdAsync(blockId, CancellationToken.None);
        
        /// <summary>
        /// Retrieves execution result for the specified block Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="FlowExecutionResult"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowExecutionResult> GetExecutionResultForBlockIdAsync(string blockId, CancellationToken cancellationToken)
        {
            try
            {
                var response = _flowApiV1.ResultsAll(new List<string> { blockId }, cancellationToken);
                return await Task.FromResult(response.ToFlowExecutionResult().FirstOrDefault());
            }
            catch (Exception ex)
            {
                throw new FlowException("Get execution result for block Id error", ex);
            }
        }

        public Task<FlowTransactionResult> WaitForSealAsync(string transactionId, int delayMs = 1000, int timeoutMs = 30000) => WaitForSealAsync(transactionId, CancellationToken.None, delayMs, timeoutMs);
        
        /// <summary>
        /// Waits for transaction result status to be sealed.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="delayMs"></param>
        /// <param name="timeoutMs"></param>
        /// <returns><see cref="FlowTransactionResult"/></returns>
        /// <exception cref="FlowException"></exception>
        public async Task<FlowTransactionResult> WaitForSealAsync(string transactionId, CancellationToken cancellationToken, int delayMs = 1000, int timeoutMs = 30000)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                var result = await GetTransactionResultAsync(transactionId, cancellationToken);

                if (result != null && result.Status == TransactionStatus.Sealed)
                    return result;

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    throw new FlowException("Timed out waiting for seal.");

                await Task.Delay(delayMs, cancellationToken);
            }
        }
    }
}