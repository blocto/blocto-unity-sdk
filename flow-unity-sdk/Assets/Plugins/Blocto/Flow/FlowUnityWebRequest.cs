
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Exceptions;
using Flow.Net.Sdk.Core.Models;
using Plugins.Blocto.Flow.Extensions;
using UnityEngine;

namespace Blocto.Flow.Client.Http.Unity
{
    public class FlowUnityWebRequest : IFlowClient
    {
        private readonly FlowApiV1 _flowApiV1;

        /// <summary>
        /// A HTTP UnityWebRequest for the Flow v1 API
        /// </summary>
        /// <param name="serverUrl"></param>
        public FlowUnityWebRequest(string serverUrl, GameObject gameObject)
        {
            _flowApiV1 = gameObject.AddComponent<FlowApiV1>();
            _flowApiV1.BaseUrl = serverUrl;
        }

        public Task PingAsync() => throw new NotImplementedException();

        public Task<FlowBlockHeader> GetLatestBlockHeaderAsync(bool isSealed = true) => throw new NotImplementedException();

        public Task<FlowBlockHeader> GetBlockHeaderByIdAsync(string blockId) => throw new NotImplementedException();

        public Task<FlowBlockHeader> GetBlockHeaderByHeightAsync(ulong height) => throw new NotImplementedException();

        public Task<FlowBlock> GetLatestBlockAsync(bool isSealed = true) => GetLatestBlockAsync(CancellationToken.None, isSealed);

        public Task<FlowBlock> GetBlockByIdAsync(string blockId) => throw new NotImplementedException();

        public Task<FlowBlock> GetBlockByHeightAsync(ulong height) => throw new NotImplementedException();

        public Task<FlowCollection> GetCollectionAsync(string collectionId) => throw new NotImplementedException();

        public Task<FlowTransactionId> SendTransactionAsync(FlowTransaction flowTransaction) => throw new NotImplementedException();

        public Task<FlowTransactionResponse> GetTransactionAsync(string transactionId) => throw new NotImplementedException();

        public Task<FlowTransactionResult> GetTransactionResultAsync(string transactionId) => throw new NotImplementedException();

        public Task<FlowAccount> GetAccountAtLatestBlockAsync(string address) => throw new NotImplementedException();

        public Task<FlowAccount> GetAccountAtBlockHeightAsync(string address, ulong blockHeight) => throw new NotImplementedException();

        public Task<ICadence> ExecuteScriptAtLatestBlockAsync(FlowScript flowScript) => throw new NotImplementedException();

        public Task<ICadence> ExecuteScriptAtBlockIdAsync(FlowScript flowScript, string blockId) => throw new NotImplementedException();

        public Task<ICadence> ExecuteScriptAtBlockHeightAsync(FlowScript flowScript, ulong blockHeight) => throw new NotImplementedException();

        public Task<IEnumerable<FlowBlockEvent>> GetEventsForHeightRangeAsync(string eventType, ulong startHeight, ulong endHeight) => throw new NotImplementedException();

        public Task<IEnumerable<FlowBlockEvent>> GetEventsForBlockIdsAsync(string eventType, IEnumerable<string> blockIds) => throw new NotImplementedException();

        public Task<FlowProtocolStateSnapshot> GetLatestProtocolStateSnapshotAsync() => throw new NotImplementedException();

        public Task<FlowExecutionResult> GetExecutionResultForBlockIdAsync(string blockId) => throw new NotImplementedException();

        public Task<FlowTransactionResult> WaitForSealAsync(string transactionId, int delayMs = 1000, int timeoutMs = 30000) => throw new NotImplementedException();

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
                var response = await _flowApiV1.BlocksAllAsync(new List<string> { isSealed ? "sealed" : "final" }, null, null, null, null, cancellationToken);
                return response.ToFlowBlock().FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new FlowException("Get latest block error", ex);
            }
        }
 
    }
}