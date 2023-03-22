using Microsoft.Extensions.Logging;
using Solnet.Rpc.Core;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Solnet.Rpc
{
    /// <summary>
    /// Implementation of the Solana streaming RPC API abstraction client.
    /// </summary>
    [DebuggerDisplay("Cluster = {" + nameof(NodeAddress) + "}")]
    internal class SolanaStreamingRpcClient : StreamingRpcClient, IStreamingRpcClient
    {
        /// <summary>
        /// Message Id generator.
        /// </summary>
        private readonly IdGenerator _idGenerator = new IdGenerator();

        /// <summary>
        /// Maps the internal ids to the unconfirmed subscription state objects.
        /// </summary>
        private readonly Dictionary<int, SubscriptionState> unconfirmedRequests = new Dictionary<int, SubscriptionState>();

        /// <summary>
        /// Maps the server ids to the confirmed subscription state objects.
        /// </summary>
        private readonly Dictionary<int, SubscriptionState> confirmedSubscriptions = new Dictionary<int, SubscriptionState>();

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="url">The url of the server to connect to.</param>
        /// <param name="logger">The possible ILogger instance.</param>
        /// <param name="websocket">The possible IWebSocket instance.</param>
        /// <param name="clientWebSocket">The possible ClientWebSocket instance.</param>
        internal SolanaStreamingRpcClient(string url, ILogger logger = null, IWebSocket websocket = default, ClientWebSocket clientWebSocket = default) : base(url, logger, websocket, clientWebSocket)
        {
        }

        /// <inheritdoc cref="StreamingRpcClient.CleanupSubscriptions"/>
        protected override void CleanupSubscriptions()
        {
            foreach (var sub in confirmedSubscriptions)
            {
                sub.Value.ChangeState(SubscriptionStatus.Unsubscribed, "Connection terminated");
            }

            foreach (var sub in unconfirmedRequests)
            {
                sub.Value.ChangeState(SubscriptionStatus.Unsubscribed, "Connection terminated");
            }
            unconfirmedRequests.Clear();
            confirmedSubscriptions.Clear();
        }


        /// <inheritdoc cref="StreamingRpcClient.HandleNewMessage(Memory{byte})"/>
        protected override void HandleNewMessage(Memory<byte> messagePayload)
        {
            //#TODO: remove and add proper logging
            string str = Encoding.UTF8.GetString(messagePayload.ToArray());
            JsonTextReader reader = new JsonTextReader(new StringReader(str));
            UnityEngine.Debug.Log($"New msg: {str}");

            string prop = "", method = "";
            int id = -1, intResult = -1;
            bool handled = false;
            bool? boolResult = null;

            while (!handled && reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        prop = reader.Value.ToString();
                        if (prop == "params")
                        {
                            HandleDataMessage(str, method);
                            handled = true;
                        }
                        break;
                    case JsonToken.String:
                        if (prop == "method")
                        {
                            method = reader.Value.ToString();
                        }
                        break;
                    case JsonToken.Integer:
                        if (prop == "id")
                        {
                            id = Convert.ToInt32(reader.Value);
                        }
                        else if (prop == "result")
                        {
                            intResult = Convert.ToInt32(reader.Value);
                        }
                        if (id != -1 && intResult != -1)
                        {
                            ConfirmSubscription(id, intResult);
                            handled = true;
                        }
                        break;
                    case JsonToken.Boolean:
                        if (prop == "result")
                        {
                            boolResult = (bool)reader.Value;
                        }
                        break;
                }
            }

            if (boolResult.HasValue)
            {
                RemoveSubscription(id, boolResult.Value);
            }
        }
        
        #region SubscriptionMapHandling
        /// <summary>
        /// Removes an unconfirmed subscription.
        /// </summary>
        /// <param name="id">The subscription id.</param>
        /// <returns>Returns the subscription object if it was found.</returns>
        private SubscriptionState RemoveUnconfirmedSubscription(int id)
        {
            SubscriptionState sub;
            lock (this)
            {
                if (!unconfirmedRequests.Remove(id, out sub))
                {
                    _logger.LogDebug(new EventId(), $"No unconfirmed subscription found with ID:{id}");
                }
            }
            return sub;
        }

        /// <summary>
        /// Removes a given subscription object from the map and notifies the object of the unsubscription.
        /// </summary>
        /// <param name="id">The subscription id.</param>
        /// <param name="shouldNotify">Whether or not to notify that the subscription was removed.</param>
        private void RemoveSubscription(int id, bool shouldNotify)
        {
            SubscriptionState sub;
            lock (this)
            {
                if (!confirmedSubscriptions.Remove(id, out sub))
                {
                    _logger.LogDebug(new EventId(), $"No subscription found with ID:{id}");
                }
            }
            if (shouldNotify)
            {
                sub?.ChangeState(SubscriptionStatus.Unsubscribed);
            }
        }

        /// <summary>
        /// Confirms a given subcription based on the internal subscription id and the newly received external id.
        /// Moves the subcription state object from the unconfirmed map to the confirmed map.
        /// </summary>
        /// <param name="internalId"></param>
        /// <param name="resultId"></param>
        private void ConfirmSubscription(int internalId, int resultId)
        {
            SubscriptionState sub;
            lock (this)
            {
                if (unconfirmedRequests.Remove(internalId, out sub))
                {
                    sub.SubscriptionId = resultId;
                    confirmedSubscriptions.Add(resultId, sub);
                }
            }

            sub?.ChangeState(SubscriptionStatus.Subscribed);
        }

        /// <summary>
        /// Adds a new subscription state object into the unconfirmed subscriptions map.
        /// </summary>
        /// <param name="subscription">The subcription to add.</param>
        /// <param name="internalId">The internally generated id of the subscription.</param>
        private void AddSubscription(SubscriptionState subscription, int internalId)
        {
            lock (this)
            {
                unconfirmedRequests.Add(internalId, subscription);
            }
        }

        /// <summary>
        /// Safely retrieves a subscription state object from a given subscription id.
        /// </summary>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns>The subscription state object.</returns>
        private SubscriptionState RetrieveSubscription(int subscriptionId)
        {
            lock (this)
            {
                return confirmedSubscriptions[subscriptionId];
            }
        }
        #endregion
        /// <summary>
        /// Handles a notification message and finishes parsing the contents.
        /// </summary>
        /// <param name="reader">The current JsonReader being used to parse the message.</param>
        /// <param name="method">The method parameter already parsed within the message.</param>
        /// <param name="subscriptionId">The subscriptionId for this message.</param>
        private void HandleDataMessage(string reader, string method)
        {
            JsonSerializerSettings opts = new JsonSerializerSettings() { MaxDepth = 64 };
            UnityEngine.Debug.Log(reader);
            switch (method)
            {
                case "accountNotification":
                    var accNotification = JsonConvert.DeserializeObject<JsonRpcWrapResponse<ResponseValue<AccountInfo>>>(reader, opts);
                    if (accNotification == null) break;

                    NotifyData(accNotification.@params.subscription, accNotification.@params.result.Value.Value);
                    break;
                case "logsNotification":
                    var logsNotification = JsonConvert.DeserializeObject<JsonRpcStreamResponse<ResponseValue<LogInfo>>>(reader);
                    if (logsNotification == null) break;
                    NotifyData(logsNotification.subscription, logsNotification.result);
                    break;
                case "programNotification":
                    var programNotification = JsonConvert.DeserializeObject<JsonRpcStreamResponse<ResponseValue<ProgramInfo>>>(reader);
                    if (programNotification == null) break;
                    NotifyData(programNotification.subscription, programNotification.result);
                    break;
                case "signatureNotification":
                    var signatureNotification = JsonConvert.DeserializeObject<JsonRpcStreamResponse<ErrorResult>>(reader);
                    if (signatureNotification == null) break;
                    NotifyData(signatureNotification.subscription, signatureNotification.result);
                    // remove subscription from map
                    break;
                case "slotNotification":
                    var slotNotification = JsonConvert.DeserializeObject<JsonRpcStreamResponse<SlotInfo>>(reader);
                    if (slotNotification == null) break;
                    NotifyData(slotNotification.subscription, slotNotification.result);
                    break;
                case "rootNotification":
                    var rootNotification = JsonConvert.DeserializeObject<JsonRpcStreamResponse<int>>(reader);
                    if (rootNotification == null) break;
                    NotifyData(rootNotification.subscription, rootNotification.result);
                    break;
            }
        }

        private void NotifyData(int subscription, object data)
        {
            var sub = RetrieveSubscription(subscription);
            sub.HandleData(data);
        }

        #region AccountInfo
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeAccountInfoAsync(string, Action{SubscriptionState, ResponseValue{AccountInfo}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeAccountInfoAsync(string pubkey, Action<SubscriptionState, ResponseValue<AccountInfo>> callback, Commitment commitment = Commitment.Finalized)

        {
            var parameters = new List<object> { pubkey };
            var configParams = new Dictionary<string, object> { { "encoding", "base64" } };

            if (commitment != Commitment.Finalized)
            {
                configParams.Add("commitment", commitment);
            }

            parameters.Add(configParams);

            var sub = new SubscriptionState<ResponseValue<AccountInfo>>(this, SubscriptionChannel.Account, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "accountSubscribe", parameters);

            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeAccountInfo(string, Action{SubscriptionState, ResponseValue{AccountInfo}}, Commitment)"/>
        public SubscriptionState SubscribeAccountInfo(string pubkey, Action<SubscriptionState, ResponseValue<AccountInfo>> callback, Commitment commitment = Commitment.Finalized)
            => SubscribeAccountInfoAsync(pubkey, callback, commitment).Result;
        #endregion

        #region TokenAccount
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeTokenAccountAsync(string, Action{SubscriptionState, ResponseValue{TokenAccountInfo}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeTokenAccountAsync(string pubkey, Action<SubscriptionState, ResponseValue<TokenAccountInfo>> callback, Commitment commitment = Commitment.Finalized)

        {
            var parameters = new List<object> { pubkey };
            var configParams = new Dictionary<string, object> { { "encoding", "jsonParsed" } };

            if (commitment != Commitment.Finalized)
            {
                configParams.Add("commitment", commitment);
            }

            parameters.Add(configParams);

            var sub = new SubscriptionState<ResponseValue<TokenAccountInfo>>(this, SubscriptionChannel.TokenAccount, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "accountSubscribe", parameters);

            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeTokenAccount(string, Action{SubscriptionState, ResponseValue{TokenAccountInfo}}, Commitment)"/>
        public SubscriptionState SubscribeTokenAccount(string pubkey, Action<SubscriptionState, ResponseValue<TokenAccountInfo>> callback, Commitment commitment = Commitment.Finalized)
            => SubscribeTokenAccountAsync(pubkey, callback, commitment).Result;
        #endregion

        #region Logs
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeLogInfoAsync(string, Action{SubscriptionState, ResponseValue{LogInfo}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeLogInfoAsync(string pubkey, Action<SubscriptionState, ResponseValue<LogInfo>> callback, Commitment commitment = Commitment.Finalized)
        {
            var parameters = new List<object> { new Dictionary<string, object> { { "mentions", new List<string> { pubkey } } } };

            if (commitment != Commitment.Finalized)
            {
                var configParams = new Dictionary<string, Commitment> { { "commitment", commitment } };
                parameters.Add(configParams);
            }

            var sub = new SubscriptionState<ResponseValue<LogInfo>>(this, SubscriptionChannel.Logs, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "logsSubscribe", parameters);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeLogInfo(string, Action{SubscriptionState, ResponseValue{LogInfo}}, Commitment)"/>
        public SubscriptionState SubscribeLogInfo(string pubkey, Action<SubscriptionState, ResponseValue<LogInfo>> callback, Commitment commitment = Commitment.Finalized)
            => SubscribeLogInfoAsync(pubkey, callback, commitment).Result;

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeLogInfoAsync(LogsSubscriptionType, Action{SubscriptionState, ResponseValue{LogInfo}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeLogInfoAsync(LogsSubscriptionType subscriptionType, Action<SubscriptionState, ResponseValue<LogInfo>> callback, Commitment commitment = Commitment.Finalized)
        {
            var parameters = new List<object> { subscriptionType };

            if (commitment != Commitment.Finalized)
            {
                var configParams = new Dictionary<string, Commitment> { { "commitment", commitment } };
                parameters.Add(configParams);
            }

            var sub = new SubscriptionState<ResponseValue<LogInfo>>(this, SubscriptionChannel.Logs, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "logsSubscribe", parameters);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeLogInfo(LogsSubscriptionType, Action{SubscriptionState, ResponseValue{LogInfo}}, Commitment)"/>
        public SubscriptionState SubscribeLogInfo(LogsSubscriptionType subscriptionType, Action<SubscriptionState, ResponseValue<LogInfo>> callback, Commitment commitment = Commitment.Finalized)
            => SubscribeLogInfoAsync(subscriptionType, callback, commitment).Result;
        #endregion

        #region Signature
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeSignatureAsync(string, Action{SubscriptionState, ResponseValue{ErrorResult}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeSignatureAsync(string transactionSignature, Action<SubscriptionState, ResponseValue<ErrorResult>> callback, Commitment commitment = Commitment.Finalized)
        {
            var parameters = new List<object> { transactionSignature };

            if (commitment != Commitment.Finalized)
            {
                var configParams = new Dictionary<string, Commitment> { { "commitment", commitment } };
                parameters.Add(configParams);
            }

            var sub = new SubscriptionState<ResponseValue<ErrorResult>>(this, SubscriptionChannel.Signature, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "signatureSubscribe", parameters);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeSignature(string, Action{SubscriptionState, ResponseValue{ErrorResult}}, Commitment)"/>
        public SubscriptionState SubscribeSignature(string transactionSignature, Action<SubscriptionState, ResponseValue<ErrorResult>> callback, Commitment commitment = Commitment.Finalized)
            => SubscribeSignatureAsync(transactionSignature, callback, commitment).Result;
        #endregion

        #region Program
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeProgramAsync(string, Action{SubscriptionState, ResponseValue{AccountKeyPair}}, Commitment)"/>
        public async Task<SubscriptionState> SubscribeProgramAsync(string programPubkey, Action<SubscriptionState, 
            ResponseValue<AccountKeyPair>> callback, Commitment commitment = Commitment.Finalized, int? dataSize = null, 
            IList<MemCmp> memCmpList = null)
        {
            List<object> filters = Parameters.Create(ConfigObject.Create(KeyValue.Create("dataSize", dataSize)));
            if (memCmpList != null)
            {
                filters ??= new List<object>();
                filters.AddRange(memCmpList.Select(filter => ConfigObject.Create(KeyValue.Create("memcmp",
                    ConfigObject.Create(KeyValue.Create("offset", filter.Offset),
                        KeyValue.Create("bytes", filter.Bytes))))));
            }
            
            List<object> parameters = Parameters.Create(
                programPubkey,
                ConfigObject.Create(
                    KeyValue.Create("encoding", "base64"),
                    KeyValue.Create("filters", filters),
                    commitment != Commitment.Finalized ? KeyValue.Create("commitment", commitment) : null));

            var sub = new SubscriptionState<ResponseValue<AccountKeyPair>>(this, SubscriptionChannel.Program, callback, parameters);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "programSubscribe", parameters);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeProgram(string, Action{SubscriptionState, ResponseValue{AccountKeyPair}}, Commitment)"/>
        public SubscriptionState SubscribeProgram(string programPubkey, Action<SubscriptionState, ResponseValue<AccountKeyPair>> callback, 
            Commitment commitment = Commitment.Finalized, int? dataSize = null, IList<MemCmp> memCmpList = null)
            => SubscribeProgramAsync(programPubkey, callback, commitment, dataSize, memCmpList).Result;
        #endregion

        #region SlotInfo
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeSlotInfoAsync(Action{SubscriptionState, SlotInfo})"/>
        public async Task<SubscriptionState> SubscribeSlotInfoAsync(Action<SubscriptionState, SlotInfo> callback)
        {
            var sub = new SubscriptionState<SlotInfo>(this, SubscriptionChannel.Slot, callback);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "slotSubscribe", null);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeSlotInfo(Action{SubscriptionState, SlotInfo})"/>
        public SubscriptionState SubscribeSlotInfo(Action<SubscriptionState, SlotInfo> callback)
            => SubscribeSlotInfoAsync(callback).Result;
        #endregion

        #region Root
        /// <inheritdoc cref="IStreamingRpcClient.SubscribeRootAsync(Action{SubscriptionState, int})"/>
        public async Task<SubscriptionState> SubscribeRootAsync(Action<SubscriptionState, int> callback)
        {
            var sub = new SubscriptionState<int>(this, SubscriptionChannel.Root, callback);

            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), "rootSubscribe", null);
            return await Subscribe(sub, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.SubscribeRoot(Action{SubscriptionState, int})"/>
        public SubscriptionState SubscribeRoot(Action<SubscriptionState, int> callback)
            => SubscribeRootAsync(callback).Result;
        #endregion

        /// <summary>
        /// Internal subscribe function, finishes the serialization and sends the message payload.
        /// </summary>
        /// <param name="sub">The subscription state object.</param>
        /// <param name="msg">The message to be serialized and sent.</param>
        /// <returns>A task representing the state of the asynchronous operation-</returns>
        private async Task<SubscriptionState> Subscribe(SubscriptionState sub, JsonRpcRequest msg)
        {
            string json = JsonConvert.SerializeObject(msg);
            var bytes = Encoding.UTF8.GetBytes(json);

            ReadOnlyMemory<byte> mem = new ReadOnlyMemory<byte>(bytes);
            await ClientSocket.SendAsync(mem, WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);

            AddSubscription(sub, msg.Id);
            return sub;
        }

        private string GetUnsubscribeMethodName(SubscriptionChannel channel) => channel switch
        {
            SubscriptionChannel.Account => "accountUnsubscribe",
            SubscriptionChannel.Logs => "logsUnsubscribe",
            SubscriptionChannel.Program => "programUnsubscribe",
            SubscriptionChannel.Root => "rootUnsubscribe",
            SubscriptionChannel.Signature => "signatureUnsubscribe",
            SubscriptionChannel.Slot => "slotUnsubscribe",
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "invalid message type")
        };

        /// <inheritdoc cref="IStreamingRpcClient.UnsubscribeAsync(SubscriptionState)"/>
        public async Task UnsubscribeAsync(SubscriptionState subscription)
        {
            var msg = new JsonRpcRequest(_idGenerator.GetNextId(), GetUnsubscribeMethodName(subscription.Channel), new List<object> { subscription.SubscriptionId });

            await Subscribe(subscription, msg).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IStreamingRpcClient.Unsubscribe(SubscriptionState)"/>
        public void Unsubscribe(SubscriptionState subscription) => UnsubscribeAsync(subscription).Wait();
    }
}