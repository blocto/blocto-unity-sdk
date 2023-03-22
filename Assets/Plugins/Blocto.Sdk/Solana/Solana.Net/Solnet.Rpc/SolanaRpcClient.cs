using Solnet.Rpc.Core;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;

namespace Solnet.Rpc
{
    /// <summary>
    /// Implements functionality to interact with the Solana JSON RPC API.
    /// </summary>
    public class SolanaRpcClient : JsonRpcClient, IRpcClient
    {
        /// <summary>
        /// Message Id generator.
        /// </summary>
        private readonly IdGenerator _idGenerator = new IdGenerator();

        /// <summary>
        /// Initialize the Rpc Client with the passed url.
        /// </summary>
        /// <param name="url">The url of the node exposing the JSON RPC API.</param>
        /// <param name="webRequestUtility">WebRequestUtility</param>
        public SolanaRpcClient(string url, WebRequestUtility webRequestUtility) : base(url, webRequestUtility)
        {
        }
        
        #region RequestBuilder

        /// <summary>
        /// Build the request for the passed RPC method and parameters.
        /// </summary>
        /// <param name="method">The request's RPC method.</param>
        /// <param name="parameters">A list of parameters to include in the request.</param>
        /// <typeparam name="T">The type of the request result.</typeparam>
        /// <returns>A task which may return a request result.</returns>
        private JsonRpcRequest BuildRequest<T>(string method, IList<object> parameters)
            => new JsonRpcRequest(_idGenerator.GetNextId(), method, parameters);

        /// <param name="method">The request's RPC method.</param>
        /// <typeparam name="T">The type of the request result.</typeparam>
        /// <returns>A task which may return a request result.</returns>
        private RequestResult<T> SendRequest<T>(string method)
        {
            JsonRpcRequest req = BuildRequest<T>(method, null);

            return SendRequest<T>(req);
        }

        /// <summary>
        /// Send a request asynchronously.
        /// </summary>
        /// <param name="method">The request's RPC method.</param>
        /// <param name="parameters">A list of parameters to include in the request.</param>
        /// <typeparam name="T">The type of the request result.</typeparam>
        /// <returns>A task which may return a request result.</returns>
        private RequestResult<T> SendRequest<T>(string method, IList<object> parameters)
        {
            try
            {
                JsonRpcRequest req = BuildRequest<T>(method, parameters);
                return SendRequest<T>(req);
            }
            catch (Exception e)
            {
                e.Message.ToLog();
                throw;
            }
        }

        private KeyValue HandleCommitment(Commitment parameter, Commitment defaultValue = Commitment.Finalized)
            => parameter != defaultValue ? KeyValue.Create("commitment", parameter) : null;

        private KeyValue HandleTransactionDetails(TransactionDetailsFilterType parameter,
            TransactionDetailsFilterType defaultValue = TransactionDetailsFilterType.Full)
            => parameter != defaultValue ? KeyValue.Create("transactionDetails", parameter) : null;

        #endregion


        #region Accounts

        public Uri NodeAddress { get; }

        /// <inheritdoc cref="IRpcClient.GetTokenMintInfo(string,Commitment)"/>
        public RequestResult<ResponseValue<TokenMintInfo>> GetTokenMintInfo(string pubKey, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<TokenMintInfo>>("getAccountInfo",
                Parameters.Create(
                    pubKey,
                    ConfigObject.Create(
                        KeyValue.Create("encoding", "jsonParsed"),
                        HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetTokenAccountInfo(string,Commitment)"/>
        public RequestResult<ResponseValue<TokenAccountInfo>> GetTokenAccountInfo(string pubKey, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<TokenAccountInfo>>("getAccountInfo",
                Parameters.Create(
                    pubKey,
                    ConfigObject.Create(
                        KeyValue.Create("encoding", "jsonParsed"),
                        HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetAccountInfo(string,Commitment,BinaryEncoding)"/>
        public RequestResult<ResponseValue<AccountInfo>> GetAccountInfo(string pubKey,
            Commitment commitment = Commitment.Finalized, BinaryEncoding encoding = BinaryEncoding.Base64)
        {
            return SendRequest<ResponseValue<AccountInfo>>("getAccountInfo",
                Parameters.Create(
                    pubKey,
                    ConfigObject.Create(
                        KeyValue.Create("encoding", encoding),
                        HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetProgramAccounts"/>
        public RequestResult<List<AccountKeyPair>> GetProgramAccounts(string pubKey,
            Commitment commitment = Commitment.Finalized, int? dataSize = null, IList<MemCmp> memCmpList = null)
        {
            List<object> filters = Parameters.Create(ConfigObject.Create(KeyValue.Create("dataSize", dataSize)));
            if (memCmpList != null)
            {
                filters ??= new List<object>();
                filters.AddRange(memCmpList.Select(filter => ConfigObject.Create(KeyValue.Create("memcmp",
                    ConfigObject.Create(KeyValue.Create("offset", filter.Offset),
                        KeyValue.Create("bytes", filter.Bytes))))));
            }

            return SendRequest<List<AccountKeyPair>>("getProgramAccounts",
                Parameters.Create(
                    pubKey,
                    ConfigObject.Create(
                        KeyValue.Create("encoding", "base64"),
                        KeyValue.Create("filters", filters),
                        HandleCommitment(commitment))));
        }


        /// <inheritdoc cref="IRpcClient.GetMultipleAccounts"/>
        public RequestResult<ResponseValue<List<AccountInfo>>> GetMultipleAccounts(IList<string> accounts, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<List<AccountInfo>>>("getMultipleAccounts",
                Parameters.Create(
                    accounts,
                    ConfigObject.Create(
                        KeyValue.Create("encoding", "base64"),
                        HandleCommitment(commitment))));
        }

        #endregion

        /// <inheritdoc cref="IRpcClient.GetBalance"/>
        public RequestResult<ResponseValue<ulong>> GetBalance(string pubKey, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<ulong>>("getBalance",
                Parameters.Create(pubKey, ConfigObject.Create(HandleCommitment(commitment))));
        }


        #region Blocks

        /// <inheritdoc cref="IRpcClient.GetBlock"/>
        public RequestResult<BlockInfo> GetBlock(ulong slot,
            Commitment commitment = Commitment.Finalized,
            TransactionDetailsFilterType transactionDetails = TransactionDetailsFilterType.Full,
            bool blockRewards = false)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<BlockInfo>("getBlock",
                Parameters.Create(slot, ConfigObject.Create(
                    KeyValue.Create("encoding", "json"),
                    HandleTransactionDetails(transactionDetails),
                    KeyValue.Create("rewards", blockRewards ? blockRewards : null),
                    HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetBlocks"/>
        public RequestResult<List<ulong>> GetBlocks(ulong startSlot, ulong endSlot = 0, Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<List<ulong>>("getBlocks",
                Parameters.Create(startSlot, endSlot > 0 ? endSlot : null,
                    ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetConfirmedBlock"/>
        public RequestResult<BlockInfo> GetConfirmedBlock(ulong slot,
            Commitment commitment = Commitment.Finalized,
            TransactionDetailsFilterType transactionDetails = TransactionDetailsFilterType.Full,
            bool blockRewards = false)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<BlockInfo>("getConfirmedBlock",
                Parameters.Create(slot, ConfigObject.Create(
                    KeyValue.Create("encoding", "json"),
                    HandleTransactionDetails(transactionDetails),
                    KeyValue.Create("rewards", blockRewards ? blockRewards : null),
                    HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetConfirmedBlocks"/>
        public RequestResult<List<ulong>> GetConfirmedBlocks(ulong startSlot, ulong endSlot = 0,
            Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<List<ulong>>("getConfirmedBlocks",
                Parameters.Create(startSlot, endSlot > 0 ? endSlot : null,
                    ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetConfirmedBlocksWithLimit"/>
        public RequestResult<List<ulong>> GetConfirmedBlocksWithLimit(ulong startSlot, ulong limit,
            Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<List<ulong>>("getConfirmedBlocksWithLimit",
                Parameters.Create(startSlot, limit, ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetBlocksWithLimit"/>
        public RequestResult<List<ulong>> GetBlocksWithLimit(ulong startSlot, ulong limit, Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
            {
                throw new ArgumentException("Commitment.Processed is not supported for this method.");
            }

            return SendRequest<List<ulong>>("getBlocksWithLimit",
                Parameters.Create(startSlot, limit, ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetFirstAvailableBlock"/>
        public RequestResult<ulong> GetFirstAvailableBlock()
        {
            return SendRequest<ulong>("getFirstAvailableBlock");
        }

        #endregion

        #region Block Production

        /// <inheritdoc cref="IRpcClient.GetBlockProduction"/>
        public RequestResult<ResponseValue<BlockProductionInfo>> GetBlockProduction(
            string identity = null, ulong? firstSlot = null, ulong? lastSlot = null,
            Commitment commitment = Commitment.Finalized)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (commitment != Commitment.Finalized)
            {
                parameters.Add("commitment", commitment);
            }

            if (!string.IsNullOrEmpty(identity))
            {
                parameters.Add("identity", identity);
            }

            if (firstSlot.HasValue)
            {
                Dictionary<string, object> range = new Dictionary<string, object> { { "firstSlot", firstSlot.Value } };

                if (lastSlot.HasValue)
                {
                    range.Add("lastSlot", lastSlot.Value);
                }

                parameters.Add("range", range);
            }
            else if (lastSlot.HasValue)
            {
                throw new ArgumentException(
                    "Range parameters are optional, but the lastSlot argument must be paired with a firstSlot.");
            }

            List<object> args = parameters.Count > 0 ? new List<object> { parameters } : null;

            return SendRequest<ResponseValue<BlockProductionInfo>>("getBlockProduction", args);
        }

        #endregion

        /// <inheritdoc cref="IRpcClient.GetHealth"/>
        public RequestResult<string> GetHealth()
        {
            return SendRequest<string>("getHealth");
        }

        /// <inheritdoc cref="IRpcClient.GetLeaderSchedule"/>
        public RequestResult<Dictionary<string, List<ulong>>> GetLeaderSchedule(ulong slot = 0,
            string identity = null, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<Dictionary<string, List<ulong>>>("getLeaderSchedule",
                Parameters.Create(
                    slot > 0 ? slot : null,
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("identity", identity))));
        }


        /// <inheritdoc cref="IRpcClient.GetTransaction"/>
        public RequestResult<TransactionMetaSlotInfo> GetTransaction(string signature,
            Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<TransactionMetaSlotInfo>("getTransaction",
                Parameters.Create(signature,
                    ConfigObject.Create(KeyValue.Create("encoding", "json"), HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetConfirmedTransaction"/>
        public RequestResult<TransactionMetaSlotInfo> GetConfirmedTransaction(string signature, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<TransactionMetaSlotInfo>("getConfirmedTransaction",
                Parameters.Create(signature,
                    ConfigObject.Create(KeyValue.Create("encoding", "json"), HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetBlockHeight"/>
        public RequestResult<ulong> GetBlockHeight(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ulong>("getBlockHeight",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetBlockCommitment"/>
        public RequestResult<BlockCommitment> GetBlockCommitment(ulong slot)
        {
            return SendRequest<BlockCommitment>("getBlockCommitment", Parameters.Create(slot));
        }

        /// <inheritdoc cref="IRpcClient.GetBlockTime"/>
        public RequestResult<ulong> GetBlockTime(ulong slot)
        {
            return SendRequest<ulong>("getBlockTime", Parameters.Create(slot));
        }

        /// <inheritdoc cref="IRpcClient.GetClusterNodes"/>
        public RequestResult<List<ClusterNode>> GetClusterNodes()
        {
            return SendRequest<List<ClusterNode>>("getClusterNodes");
        }

        /// <inheritdoc cref="IRpcClient.GetEpochInfo"/>
        public RequestResult<EpochInfo> GetEpochInfo(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<EpochInfo>("getEpochInfo",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetEpochSchedule"/>
        public RequestResult<EpochScheduleInfo> GetEpochSchedule()
        {
            return SendRequest<EpochScheduleInfo>("getEpochSchedule");
        }

        /// <inheritdoc cref="IRpcClient.GetFeeCalculatorForBlockhash"/>
        public RequestResult<ResponseValue<FeeCalculatorInfo>> GetFeeCalculatorForBlockhash(
            string blockhash, Commitment commitment = Commitment.Finalized)
        {
            List<object> parameters = Parameters.Create(blockhash, ConfigObject.Create(HandleCommitment(commitment)));

            return SendRequest<ResponseValue<FeeCalculatorInfo>>("getFeeCalculatorForBlockhash", parameters);
        }


        /// <inheritdoc cref="IRpcClient.GetFeeRateGovernor"/>
        public RequestResult<ResponseValue<FeeRateGovernorInfo>> GetFeeRateGovernor()
        {
            return SendRequest<ResponseValue<FeeRateGovernorInfo>>("getFeeRateGovernor");
        }

        /// <inheritdoc cref="IRpcClient.GetFees"/>
        public RequestResult<ResponseValue<FeesInfo>> GetFees(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<FeesInfo>>("getFees",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetFeeForMessage"/>
        public RequestResult<ResponseValue<ulong>> GetFeeForMessage(string message, Commitment commitment = Commitment.Finalized)
        {
            List<object> parameters = Parameters.Create(message, ConfigObject.Create(HandleCommitment(commitment)));
            return SendRequest<ResponseValue<ulong>>("getFeeForMessage", parameters);
        }

        /// <inheritdoc cref="IRpcClient.GetRecentBlockHash"/>
        public RequestResult<ResponseValue<BlockHash>> GetRecentBlockHash(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<BlockHash>>("getRecentBlockhash",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetLatestBlockHash"/>
        public RequestResult<ResponseValue<LatestBlockHash>> GetLatestBlockHash(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<LatestBlockHash>>("getLatestBlockhash",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.IsBlockHashValid"/>
        public RequestResult<ResponseValue<bool>> IsBlockHashValid(string blockHash, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<bool>>("isBlockhashValid",
                Parameters.Create(blockHash, ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetMaxRetransmitSlot"/>
        public RequestResult<ulong> GetMaxRetransmitSlot()
        {
            return SendRequest<ulong>("getMaxRetransmitSlot");
        }

        /// <inheritdoc cref="IRpcClient.GetMaxShredInsertSlot"/>
        public RequestResult<ulong> GetMaxShredInsertSlot()
        {
            return SendRequest<ulong>("getMaxShredInsertSlot");
        }

        /// <inheritdoc cref="IRpcClient.GetMinimumBalanceForRentExemption"/>
        public RequestResult<ulong> GetMinimumBalanceForRentExemption(long accountDataSize,
            Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ulong>("getMinimumBalanceForRentExemption",
                Parameters.Create(accountDataSize, ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetGenesisHash"/>
        public RequestResult<string> GetGenesisHash()
        {
            return SendRequest<string>("getGenesisHash");
        }


        /// <inheritdoc cref="IRpcClient.GetIdentity"/>
        public RequestResult<NodeIdentity> GetIdentity()
        {
            return SendRequest<NodeIdentity>("getIdentity");
        }

        /// <inheritdoc cref="IRpcClient.GetInflationGovernor"/>
        public RequestResult<InflationGovernor> GetInflationGovernor(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<InflationGovernor>("getInflationGovernor",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetInflationRate"/>
        public RequestResult<InflationRate> GetInflationRate()
        {
            return SendRequest<InflationRate>("getInflationRate");
        }


        /// <inheritdoc cref="IRpcClient.GetInflationReward"/>
        public RequestResult<List<InflationReward>> GetInflationReward(IList<string> addresses,
            ulong epoch = 0, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<List<InflationReward>>("getInflationReward",
                Parameters.Create(
                    addresses,
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("epoch", epoch > 0 ? epoch : null))));
        }


        /// <inheritdoc cref="IRpcClient.GetLargestAccounts"/>
        public RequestResult<ResponseValue<List<LargeAccount>>> GetLargestAccounts(
            AccountFilterType? filter = null,
            Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<List<LargeAccount>>>("getLargestAccounts",
                Parameters.Create(
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("filter", filter))));
        }

        /// <inheritdoc cref="IRpcClient.GetSnapshotSlot"/>
        public RequestResult<ulong> GetSnapshotSlot()
        {
            return SendRequest<ulong>("getSnapshotSlot");
        }

        /// <inheritdoc cref="IRpcClient.GetHighestSnapshotSlot"/>
        public RequestResult<SnapshotSlotInfo> GetHighestSnapshotSlot()
        {
            return SendRequest<SnapshotSlotInfo>("getHighestSnapshotSlot");
        }

        /// <inheritdoc cref="IRpcClient.GetRecentPerformanceSamples"/>
        public RequestResult<List<PerformanceSample>> GetRecentPerformanceSamples(ulong limit = 720)
        {
            return SendRequest<List<PerformanceSample>>("getRecentPerformanceSamples",
                new List<object> { limit });
        }

        /// <inheritdoc cref="IRpcClient.GetSignaturesForAddress"/>
        public RequestResult<List<SignatureStatusInfo>> GetSignaturesForAddress(string accountPubKey,
            ulong limit = 1000, string before = null, string until = null, Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
               throw new ArgumentException("Commitment.Processed is not supported for this method.");

            return SendRequest<List<SignatureStatusInfo>>("getSignaturesForAddress",
                Parameters.Create(
                    accountPubKey,
                    ConfigObject.Create(
                        KeyValue.Create("limit", limit != 1000 ? limit : null),
                        KeyValue.Create("before", before),
                        KeyValue.Create("until", until),
                        HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetConfirmedSignaturesForAddress2"/>
        public RequestResult<List<SignatureStatusInfo>> GetConfirmedSignaturesForAddress2(
            string accountPubKey, ulong limit = 1000, string before = null, string until = null,
            Commitment commitment = Commitment.Finalized)
        {
            if (commitment == Commitment.Processed)
                throw new ArgumentException("Commitment.Processed is not supported for this method.");

            return SendRequest<List<SignatureStatusInfo>>("getConfirmedSignaturesForAddress2",
                Parameters.Create(
                    accountPubKey,
                    ConfigObject.Create(
                        KeyValue.Create("limit", limit != 1000 ? limit : null),
                        KeyValue.Create("before", before),
                        KeyValue.Create("until", until),
                        HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetSignatureStatuses"/>
        public RequestResult<ResponseValue<List<SignatureStatusInfo>>> GetSignatureStatuses(
            List<string> transactionHashes,
            bool searchTransactionHistory = false)
        {
            return SendRequest<ResponseValue<List<SignatureStatusInfo>>>("getSignatureStatuses",
                Parameters.Create(
                    transactionHashes,
                    ConfigObject.Create(
                        KeyValue.Create("searchTransactionHistory",
                            searchTransactionHistory ? searchTransactionHistory : null))));
        }

        /// <inheritdoc cref="IRpcClient.GetSlot"/>
        public RequestResult<ulong> GetSlot(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ulong>("getSlot",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetSlotLeader"/>
        public RequestResult<string> GetSlotLeader(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<string>("getSlotLeader", Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetSlotLeaders"/>
        public RequestResult<List<string>> GetSlotLeaders(ulong start, ulong limit, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<List<string>>("getSlotLeaders",
                Parameters.Create(start, limit, ConfigObject.Create(HandleCommitment(commitment))));
        }

        #region Token Supply and Balances

        /// <inheritdoc cref="IRpcClient.GetStakeActivation"/>
        public RequestResult<StakeActivationInfo> GetStakeActivation(string publicKey, ulong epoch = 0, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<StakeActivationInfo>("getStakeActivation",
                Parameters.Create(
                    publicKey,
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("epoch", epoch > 0 ? epoch : null))));
        }


        /// <inheritdoc cref="IRpcClient.GetSupply"/>
        public RequestResult<ResponseValue<Supply>> GetSupply( Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<Supply>>("getSupply",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetTokenAccountBalance"/>
        public RequestResult<ResponseValue<TokenBalance>> GetTokenAccountBalance(
            string splTokenAccountPublicKey, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<TokenBalance>>("getTokenAccountBalance",
                Parameters.Create(splTokenAccountPublicKey, ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetTokenAccountsByDelegate"/>
        public RequestResult<ResponseValue<List<TokenAccount>>> GetTokenAccountsByDelegate(
            string ownerPubKey, string tokenMintPubKey = null, string tokenProgramId = null,
            Commitment commitment = Commitment.Finalized)
        {
            if (string.IsNullOrWhiteSpace(tokenMintPubKey) && string.IsNullOrWhiteSpace(tokenProgramId))
                throw new ArgumentException("either tokenProgramId or tokenMintPubKey must be set");

            return SendRequest<ResponseValue<List<TokenAccount>>>("getTokenAccountsByDelegate",
                Parameters.Create(
                    ownerPubKey,
                    ConfigObject.Create(
                        KeyValue.Create("mint", tokenMintPubKey),
                        KeyValue.Create("programId", tokenProgramId)),
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("encoding", "jsonParsed"))));
        }


        /// <inheritdoc cref="IRpcClient.GetTokenAccountsByOwner"/>
        public RequestResult<ResponseValue<List<TokenAccount>>> GetTokenAccountsByOwner(
            string ownerPubKey, string tokenMintPubKey = null, string tokenProgramId = null,
            Commitment commitment = Commitment.Finalized)
        {
            if (string.IsNullOrWhiteSpace(tokenMintPubKey) && string.IsNullOrWhiteSpace(tokenProgramId))
                throw new ArgumentException("either tokenProgramId or tokenMintPubKey must be set");

            return SendRequest<ResponseValue<List<TokenAccount>>>("getTokenAccountsByOwner",
                Parameters.Create(
                    ownerPubKey,
                    ConfigObject.Create(
                        KeyValue.Create("mint", tokenMintPubKey),
                        KeyValue.Create("programId", tokenProgramId)),
                    ConfigObject.Create(
                        HandleCommitment(commitment),
                        KeyValue.Create("encoding", "jsonParsed"))));
        }

        /// <inheritdoc cref="IRpcClient.GetTokenLargestAccounts"/>
        public RequestResult<ResponseValue<List<LargeTokenAccount>>> GetTokenLargestAccounts(
            string tokenMintPubKey, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<List<LargeTokenAccount>>>("getTokenLargestAccounts",
                Parameters.Create(tokenMintPubKey, ConfigObject.Create(HandleCommitment(commitment))));
        }


        /// <inheritdoc cref="IRpcClient.GetTokenSupply"/>
        public RequestResult<ResponseValue<TokenBalance>> GetTokenSupply(string tokenMintPubKey,
            Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ResponseValue<TokenBalance>>("getTokenSupply",
                Parameters.Create(tokenMintPubKey, ConfigObject.Create(HandleCommitment(commitment))));
        }

        #endregion

        /// <inheritdoc cref="IRpcClient.GetTransactionCount"/>
        public RequestResult<ulong> GetTransactionCount(Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<ulong>("getTransactionCount",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment))));
        }

        /// <inheritdoc cref="IRpcClient.GetVersion"/>
        public RequestResult<NodeVersion> GetVersion()
        {
            return SendRequest<NodeVersion>("getVersion");
        }

        /// <inheritdoc cref="IRpcClient.GetVoteAccounts"/>
        public RequestResult<VoteAccounts> GetVoteAccounts(string votePubKey = null, Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<VoteAccounts>("getVoteAccounts",
                Parameters.Create(ConfigObject.Create(HandleCommitment(commitment),
                    KeyValue.Create("votePubkey", votePubKey))));
        }

        /// <inheritdoc>
        ///     <cref>IRpcClient.GetMinimumLedgerSlot</cref>
        /// </inheritdoc>
        public RequestResult<ulong> GetMinimumLedgerSlot()
        {
            return SendRequest<ulong>("minimumLedgerSlot");
        }

        /// <inheritdoc cref="IRpcClient.RequestAirdrop"/>
        public RequestResult<string> RequestAirdrop(string pubKey, ulong lamports,
            Commitment commitment = Commitment.Finalized)
        {
            return SendRequest<string>("requestAirdrop",
                Parameters.Create(pubKey, lamports, ConfigObject.Create(HandleCommitment(commitment))));
        }
        
        #region Transactions

        /// <inheritdoc cref="IRpcClient.SendTransaction(byte[], bool, Commitment)"/>
        public RequestResult<string> SendTransaction(byte[] transaction, bool skipPreflight = false,
            Commitment preflightCommitment = Commitment.Finalized)
        {
            return SendTransaction(Convert.ToBase64String(transaction), skipPreflight, preflightCommitment);
        }


        /// <inheritdoc cref="IRpcClient.SendTransaction(string, bool, Commitment)"/>
        public RequestResult<string> SendTransaction(string transaction, bool skipPreflight = false, Commitment preflightCommitment = Commitment.Finalized)
        {
            return SendRequest<string>("sendTransaction",
                Parameters.Create(
                    transaction,
                    ConfigObject.Create(
                        KeyValue.Create("skipPreflight", skipPreflight ? skipPreflight : null),
                        KeyValue.Create("preflightCommitment",
                            preflightCommitment == Commitment.Finalized ? null : preflightCommitment),
                        KeyValue.Create("encoding", BinaryEncoding.Base64))));
        }

        /// <inheritdoc cref="IRpcClient.SimulateTransaction(string, bool, Commitment, bool, IList{string})"/>
        public RequestResult<ResponseValue<SimulationLogs>> SimulateTransaction(string transaction,
            bool sigVerify = false,
            Commitment commitment = Commitment.Finalized, bool replaceRecentBlockhash = false,
            IList<string> accountsToReturn = null)
        {
            if (sigVerify && replaceRecentBlockhash)
            {
                throw new ArgumentException(
                    $"Parameters {nameof(sigVerify)} and {nameof(replaceRecentBlockhash)} are incompatible, only one can be set to true.");
            }

            return SendRequest<ResponseValue<SimulationLogs>>("simulateTransaction",
                Parameters.Create(
                    transaction,
                    ConfigObject.Create(
                        KeyValue.Create("sigVerify", sigVerify ? sigVerify : null),
                        HandleCommitment(commitment),
                        KeyValue.Create("encoding", BinaryEncoding.Base64),
                        KeyValue.Create("replaceRecentBlockhash",
                            replaceRecentBlockhash ? replaceRecentBlockhash : null),
                        KeyValue.Create("accounts", accountsToReturn != null
                            ? ConfigObject.Create(
                                KeyValue.Create("encoding", BinaryEncoding.Base64),
                                KeyValue.Create("addresses", accountsToReturn))
                            : null))));
        }

        /// <inheritdoc cref="IRpcClient.SimulateTransaction(byte[], bool, Commitment, bool, IList{string})"/>
        public RequestResult<ResponseValue<SimulationLogs>> SimulateTransaction(byte[] transaction,
            bool sigVerify = false,
            Commitment commitment = Commitment.Finalized, bool replaceRecentBlockhash = false,
            IList<string> accountsToReturn = null)
            => SimulateTransaction(transaction, sigVerify, commitment, replaceRecentBlockhash, accountsToReturn);

        #endregion

        /// <summary>
        /// Gets the id for the next request.
        /// </summary>
        /// <returns>The id.</returns>
        int IRpcClient.GetNextIdForReq() => _idGenerator.GetNextId();

    }
}