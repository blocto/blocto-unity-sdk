using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.FCL.Extension;
using Flow.FCL.Models;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL
{
    public class FlowClientLibrary : MonoBehaviour
    {
        public static Config.Config Config { get; private set; }

        private CurrentUser _currentUser;
        
        private IFlowClient _flowClient;
        
        private Transaction _transaction;
        
        private ICadence _response;
        
        private string _errorMessage;
        
        private bool _isSuccessed;
        
        public static FlowClientLibrary CreateClientLibrary(GameObject gameObject, IWalletProvider walletProvider, Config.Config config = null)
        {
            Config = config;
            FlowClientLibrary.ConfigSetDefaultValue();
            
            var env = Config.Get("flow.network");
            var fcl = gameObject.AddComponent<FlowClientLibrary>();
            var flowClient = new FlowUnityWebRequest(gameObject, config.Get("accessNode.api")); 
            var factory = UtilFactory.CreateUtilFactory(gameObject, flowClient);
            var appUtil = factory.CreateAppUtil(env);
            var resolveUtility = factory.CreateResolveUtility();
            var currentUser = new CurrentUser(walletProvider, factory.CreateWebRequestUtil(), resolveUtility, flowClient, appUtil);
            
            fcl._flowClient = flowClient;
            fcl._transaction = new Transaction(
                walletProvider,
                flowClient,
                resolveUtility,
                factory);
            fcl._currentUser = currentUser;
            
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
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(Action<CurrentUser, FlowAccount> callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _currentUser.Authenticate(url, callback);
        }
        
        public void Login(Action<CurrentUser, FlowAccount> callback = null)
        {
            Authenticate(callback);
        }
        
        /// <summary>
        /// As the current user Mutate the Flow Blockchain
        /// </summary>
        /// <param name="tx">FlowTransaction</param>
        /// <param name="callback"></param>
        public void Mutate(FlowTransaction tx, Action<string> callback)
        {
            var service = _currentUser.Services.FirstOrDefault(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            if(service is null)
            {
                throw new Exception("Please connect wallet first.");
                
            }
            
            _transaction.SendTransaction(service.PreAuthzEndpoint(), tx, () => {}, callback);
        }
        
        public void SignUserMessage(string message, Action<ExecuteResult<List<(string Source, string Signature, ulong KeyId)>>> callback = null)
        {
            _currentUser.SignUserMessage(message, callback);
        }
        
        /// <summary>
        /// Query the Flow Blockchain
        /// </summary>
        /// <param name="flowScript">Flow cadence script and arguments</param>
        /// <returns>QueryResult</returns>
        public async Task<ExecuteResult<ICadence>> Query(FlowScript flowScript)
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
        
        public ExecuteResult<(TransactionExecution Execution, TransactionStatus Status, string BlockId)> GetTransactionReuslt(string transactionId)
        {
            var txr = _transaction.GetTransactionResult(transactionId);
            var result = txr.Execution switch
                         {
                             TransactionExecution.Failure => new ExecuteResult<(TransactionExecution Execution, TransactionStatus Status, string BlockId)>
                                                             {
                                                                 Data = (TransactionExecution.Failure, TransactionStatus.Sealed, string.Empty), IsSuccessed = true,
                                                                 Message = txr.ErrorMessage
                                                             },
                             TransactionExecution.Success => new ExecuteResult<(TransactionExecution Execution, TransactionStatus Status, string BlockId)>
                                                             {
                                                                 Data = (TransactionExecution.Success, TransactionStatus.Sealed, txr.BlockId), IsSuccessed = true,
                                                                 Message = string.Empty
                                                             },
                             TransactionExecution.Pending => new ExecuteResult<(TransactionExecution Execution, TransactionStatus Status, string BlockId)>
                                                             {
                                                                 Data = (TransactionExecution.Pending, TransactionStatus.Pending, string.Empty), IsSuccessed = true,
                                                                 Message = "Still Pending"
                                                             },
                             _ => throw new ArgumentOutOfRangeException()
                         }; 
            return result;
        }
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _transaction.GetAccount(address);
            return await account;
        }

        private async Task ExecuteScript(FlowScript flowScript)
        {
            try
            {
                _response = await _flowClient.ExecuteScriptAtLatestBlockAsync(flowScript); 
                _isSuccessed = true;
            }
            catch (Exception e)
            {
                $"Execute script error: {e.Message}".ToLog();
                _errorMessage = e.Message;
            }
        }
        
        private static void ConfigSetDefaultValue()
        {
            FlowClientLibrary.Config.Put("testnet.fclcrypto", "0x5b250a8a85b44a67");
            FlowClientLibrary.Config.Put("mainnet.fclcrypto", "0xdb6b70764af4ff68");
        }
    }
}