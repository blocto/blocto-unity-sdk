using System;
using System.Linq;
using System.Threading.Tasks;
using Flow.FCL.Extensions;
using Flow.FCL.Models;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Plugins.Flow.FCL.Models;
using UnityEngine;

namespace Flow.FCL
{
    public class FlowClientLibrary : MonoBehaviour
    {
        public IFlowClient FlowClient { get; set; }
        
        public static Config.Config Config { get; private set; }

        private CurrentUser _currentUser;
        
        private Transaction _transaction;
        
        private ICadence _response;
        
        private IResolveUtility _resolveUtility;
        
        private IWalletProvider _walletProvider;
        
        private string _errorMessage;
        
        private bool _isSuccessed;
        
        public static FlowClientLibrary CreateClientLibrary(GameObject gameObject, Func<Func<FlowClientLibrary>,FlowClientLibrary> initialFun, Config.Config config = null)
        {
            if(config != null)
            {
                Config = config;
            }
            
            var env = Config.Get("flow.network");
            var fcl = initialFun.Invoke(() => {
                                            var tmpFcl = gameObject.AddComponent<FlowClientLibrary>();
                                            var flowClient = new FlowUnityWebRequest(gameObject, Config.Get("accessNode.api")); 
                                            tmpFcl.FlowClient = flowClient;
                                            return tmpFcl;
                                        });
            
            var factory = UtilFactory.CreateUtilFactory(gameObject, fcl.FlowClient, fcl._resolveUtility);
            fcl._currentUser = new CurrentUser(fcl._walletProvider, factory.CreateWebRequestUtil(), fcl._resolveUtility, fcl.FlowClient); 
            fcl._transaction = new Transaction(fcl._walletProvider, fcl.FlowClient, fcl._resolveUtility, factory);
            return fcl;
        }
        
        static FlowClientLibrary()
        {
            Config = new Config.Config();
        }
        
        private FlowClientLibrary()
        {
           _response = default(Cadence);
           _errorMessage = string.Empty;
           _isSuccessed = false;
        }

        /// <summary>
        /// Get CurrentUser
        /// </summary>
        /// <returns>CurrentUser</returns>
        public CurrentUser CurrentUser()
        {
            return _currentUser;
        }
        
        /// <summary>
        /// Set fcl wallet provider
        /// </summary>
        /// <param name="walletProvider">IWalletProvider implement</param>
        public void SetWalletProvider(IWalletProvider walletProvider)
        {
            _walletProvider = walletProvider;
        }
        
        /// <summary>
        /// Set fcl IResolveUtility
        /// </summary>
        /// <param name="resolveUtility">ResolveUtility implement</param>
        public void SetResolveUtility(IResolveUtility resolveUtility)
        {
            _resolveUtility = resolveUtility;
        }
        
        
        /// <summary>
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(Action<CurrentUser, AccountProofData> callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _currentUser.Authenticate(url, callback);
        }
        
        /// <summary>
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="accountProofData">Account proof data</param>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(AccountProofData accountProofData, Action<CurrentUser, AccountProofData> callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _currentUser.Authenticate(url, accountProofData, callback);
        }
        
        /// <summary>
        /// Logs out the current user and sets the values on the current user object to null.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void UnAuthenticate(Action callback = null)
        {
            this._currentUser.Services = null;
            this._currentUser.LoggedIn = false;
            this._currentUser.ExpiresAt = default;
            callback?.Invoke();
        }
        
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Login(Action<CurrentUser, AccountProofData> callback = null)
        {
            Authenticate(callback);
        }
        
        /// <summary>
        /// ReLogin
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void ReLogin(Action<CurrentUser, AccountProofData> callback = null)
        {
            UnAuthenticate();
            Authenticate(callback);
        }
        
        /// <summary>
        /// As the current user Mutate the Flow Blockchain
        /// </summary>
        /// <param name="tx">FlowTransaction</param>
        /// <param name="callback">The callback will be called when the send transaction completed, making it easy to update the UI accordingly.</param>
        public void Mutate(FlowTransaction tx, Action<string> callback)
        {
            var service = _currentUser.Services.FirstOrDefault(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            if(service is null)
            {
                throw new Exception("Please connect wallet first.");
                
            }
            
            _transaction.SendTransaction(service.PreAuthzEndpoint(), tx, () => {}, callback);
        }
        
        /// <summary>
        /// Sign user message
        /// </summary>
        /// <param name="message">Source message</param>
        /// <param name="callback">Complete sign message then call callback function</param>
        public void SignUserMessage(string message, Action<ExecuteResult<FlowSignature>> callback = null)
        {
            _currentUser.SignUserMessage(message, callback);
        }
        
        /// <summary>
        /// Query the Flow Blockchain
        /// </summary>
        /// <param name="flowScript">Flow cadence script and arguments</param>
        /// <returns>QueryResult</returns>
        public async Task<ExecuteResult<ICadence>> QueryAsync(FlowScript flowScript)
        {
            await ExecuteScript(flowScript);
            var result = new ExecuteResult<ICadence>
                         {
                             Data = _response,
                             IsSuccessed = _isSuccessed,
                             Message = _errorMessage
                         };
            _isSuccessed = false;
            _errorMessage = string.Empty;
            _response = default(Cadence);
            return result;
        }
        
        /// <summary>
        /// Get transaction result
        /// </summary>
        /// <param name="transactionId">Transaction hash code</param>
        /// <returns>Tuple include Execution, Status, BlockId</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ExecuteResult<FlowTransactionResult> GetTransactionStatus(string transactionId)
        {
            var txr = _transaction.GetTransactionStatus(transactionId);
            var result = txr.Execution switch
                         {
                             TransactionExecution.Failure => new ExecuteResult<FlowTransactionResult>
                                                             {
                                                                 Data = txr,
                                                                 IsSuccessed = true,
                                                                 Message = txr.ErrorMessage
                                                             },
                             TransactionExecution.Success => new ExecuteResult<FlowTransactionResult>
                                                             {
                                                                 Data = txr,
                                                                 IsSuccessed = true,
                                                                 Message = string.Empty
                                                             },
                             TransactionExecution.Pending => new ExecuteResult<FlowTransactionResult>
                                                             {
                                                                 Data = txr,
                                                                 IsSuccessed = true,
                                                                 Message = "Still Pending"
                                                             },
                             _ => throw new ArgumentOutOfRangeException()
                         }; 
            
            return result;
        }
        
        private async Task ExecuteScript(FlowScript flowScript)
        {
            try
            {
                _response = await FlowClient.ExecuteScriptAtLatestBlockAsync(flowScript); 
                _isSuccessed = true;
            }
            catch (Exception e)
            {
                $"Execute script error: {e.Message}".ToLog();
                _errorMessage = e.Message;
            }
        }
    }
}