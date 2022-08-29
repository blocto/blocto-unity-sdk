using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blocto.Flow;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core.Models;
using UnityEngine;

namespace Flow.FCL
{
    public class FlowClientLibrary : MonoBehaviour
    {
        private Config.Config Config { get; set; }

        private IWalletProvider _walletProvider;
        
        private CurrentUser _currentUser;
        
        private IWebRequestUtils _webRequestUtils;
        
        private CoreModule _coreModule;
        
        private IResolveUtils _resolveUtils;
        
        public static FlowClientLibrary CreateClientLibrary(GameObject gameObject, Config.Config config, string mode, IResolveUtils resolveUtils)
        {
            var fcl = gameObject.AddComponent<FlowClientLibrary>();
            var factory = gameObject.AddComponent<UtilFactory>();
            var tmpWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(gameObject, config.Get("testnet"));
            fcl._resolveUtils = resolveUtils;
            fcl._walletProvider = tmpWalletProvider;
            fcl._webRequestUtils = factory.CreateWebRequestUtil();
            fcl._coreModule = new CoreModule(
                tmpWalletProvider,
                new FlowUnityWebRequest(gameObject, config.Get("testnet")), 
                resolveUtils,
                factory);
            fcl.Config = config;
            return fcl;
        }
        
        public FlowClientLibrary()
        {
           Config = new Config.Config();
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
        public void Authenticate(Action callback = null)
        {
            var url = Config.Get("discovery.wallet");
            _coreModule.Authenticate(url, () => {
                                                           Debug.Log("In internal callback");
                                                           Debug.Log($"WallProvider is null {_walletProvider == null}");
                                                           switch (_walletProvider.PollingResponse.Status)
                                                           {
                                                               case PollingStatusEnum.APPROVED:
                                                                   Debug.Log($"Polling response status APPROVED");
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                      Addr = new FlowAddress(_walletProvider.PollingResponse.Data.Addr),
                                                                                      LoggedIn = true, 
                                                                                      F_type = "USER",
                                                                                      F_vsn = _walletProvider.PollingResponse.FVsn,
                                                                                      Services = _walletProvider.PollingResponse.Data.Services.ToList(),
                                                                                      ExpiresAt = _walletProvider.PollingResponse.Data.Expires 
                                                                                  };
                                                                   
                                                                   _currentUser.SetConfig(Config);
                                                                   _currentUser.SetWalletProvider(_walletProvider);
                                                                   _currentUser.SetCoreModule(_coreModule);
                                                                   _currentUser.SetCurrentUser(_currentUser);
                                                                   Debug.Log($"currentUser service count: {_currentUser.Services.Count}");
                                                                   break;
                                                               case PollingStatusEnum.DECLINED:
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                       LoggedIn = false, 
                                                                                       F_type = "USER",
                                                                                       F_vsn = _walletProvider.PollingResponse.FVsn,
                                                                                       ExpiresAt = _walletProvider.PollingResponse.Data.Expires
                                                                                  };
                                                                   break;
                                                               case PollingStatusEnum.PENDING:
                                                               case PollingStatusEnum.REDIRECT:
                                                               case PollingStatusEnum.NONE:
                                                               default:
                                                                   break;
                                                           }
                                                       }, callback);
        }
        
        public void PreAuthz(PreSignable preSignable, FlowTransaction tx)
        {
            var service = _currentUser.Services.First(p => p.Type == ServiceTypeEnum.PREAUTHZ);
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(service.Endpoint.AbsoluteUri + "?")
                      .Append(Uri.EscapeUriString("sessionId") + "=")
                      .Append(Uri.EscapeDataString(service.PollingParams.SessionId));
            
            Debug.Log($"Pre authz url: {urlBuilder.ToString()}");
            _coreModule.PreAuthz(preSignable, service, tx, null);
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
    }
}