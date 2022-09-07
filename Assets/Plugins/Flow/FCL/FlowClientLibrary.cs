using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Blocto.Flow;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Models;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
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

        private IWalletProvider _walletProvider;
        
        private CurrentUser _currentUser;
        
        private IFlowClient _flowClient;
        
        private CoreModule _coreModule;
        
        private ICadence _response;
        
        private string _errorMessage;
        
        private bool _isSuccessed;
        
        public static FlowClientLibrary CreateClientLibrary(GameObject gameObject, Config.Config config, string env, IResolveUtility resolveUtility)
        {
            var fcl = gameObject.AddComponent<FlowClientLibrary>();
            var tmpWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(gameObject, config.Get("testnet"));
            var flowClient = new FlowUnityWebRequest(gameObject, config.Get(env)); 
            var factory = UtilFactory.CreateUtilFactory(gameObject, flowClient);
            var appUtil = factory.CreateAppUtil();
            var currentUser = new CurrentUser(tmpWalletProvider, factory.CreateWebRequestUtil(), resolveUtility, appUtil);
            
            fcl._flowClient = flowClient;
            fcl._walletProvider = tmpWalletProvider;
            fcl._coreModule = new CoreModule(
                tmpWalletProvider,
                flowClient,
                resolveUtility,
                factory);
            fcl._currentUser = currentUser;
            Config = config;
            
            FlowClientLibrary.ConfigSetDefaultValue();
            return fcl;
        }
        
        private FlowClientLibrary()
        {
           Config = new Config.Config();
           _response = default(Cadence);
           _errorMessage = string.Empty;
           _isSuccessed = false;
        }

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
        public void Authenticate(Action<CurrentUser> callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _currentUser.Authenticate(url, callback);
        }
        
        public void Mutate(FlowTransaction tx, Action<string> callback)
        {
            var service = _currentUser.Services.First(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            tx = _coreModule.SendTransaction(service, tx, () => {}, callback);
        }
        
        public void SignUserMessage(string message, Action<SignMessageResponse> callback = null)
        {
            _currentUser.SignUserMessage(message, callback);
        }
        
        public async Task<QueryResult> Query(FlowScript flowScript)
        {
            await ExecuteScript(flowScript);
            var result = new QueryResult
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
            this._currentUser = null;
            callback?.Invoke();
        }
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _coreModule.GetAccount(address);
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
        
        private IEnumerator WaitForSeconds(float time)
        {
            yield return new WaitForSeconds(time);
        }
        
        private static void ConfigSetDefaultValue()
        {
            FlowClientLibrary.Config.Put("testnet.fclcrypto", "0x5b250a8a85b44a67");
            FlowClientLibrary.Config.Put("mainnet.fclcrypto", "0xdb6b70764af4ff68");
        }
    }
}