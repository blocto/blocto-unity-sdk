using System;
using System.Linq;
using System.Text;
using Flow.FCL.Models;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core.Models;
using UnityEngine;

namespace Flow.FCL
{
    public class FlowClientLibrary : MonoBehaviour
    {
        public Config.Config Config { get; set; }

        public IWalletProvider WalletProvider { get; set; }
        
        private CurrentUser _currentUser;
        
        private IWebRequestUtils _webRequestUtils;
        
        public FlowClientLibrary()
        {
            Config = new Config.Config();
        }

        public void Start()
        {
            var factory = gameObject.AddComponent<HelperFactory>();
            _webRequestUtils = factory.CreateWebRequestHelper();
        }

        /// <summary>
        /// Set custom webrequest helper
        /// </summary>
        /// <param name="webRequestUtils">IWebRequestHelper</param>
        public void SetWebRequestHelper(IWebRequestUtils webRequestUtils)
        {
            _webRequestUtils = webRequestUtils;
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
            var authnResponse = _webRequestUtils.GetResponse<AuthnResponse>(url);  
            
            var authnUrlBuilder = new StringBuilder();
            authnUrlBuilder.Append(authnResponse.AuthnLocal.Endpoint + "?")
                           .Append(Uri.EscapeDataString("channel") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.Channel) + "&")
                           .Append(Uri.EscapeDataString("authenticationId") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.AuthenticationId) + "&")
                           .Append(Uri.EscapeDataString("fclVersion") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.FclVersion));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(authnResponse.AuthnUpdates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("authenticationId") + "=")
                             .Append(Uri.EscapeDataString(authnResponse.AuthnUpdates.Params.AuthenticationId));
            var authnUrl = authnUrlBuilder.ToString();
            var pollingUrl = pollingUrlBuilder.ToString();
            Debug.Log($"Authn Url: {authnUrl}, Polling Url:{pollingUrl}");
            WalletProvider.Login(authnUrl, pollingUrl, () => {
                                                           switch (WalletProvider.PollingResponse.Status)
                                                           {
                                                               case PollingStatusEnum.APPROVED:
                                                                   Debug.Log($"Polling response status APPROVED");
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                      Addr = new FlowAddress(WalletProvider.PollingResponse.Data.Addr),
                                                                                      LoggedIn = true, 
                                                                                      F_type = "USER",
                                                                                      F_vsn = WalletProvider.PollingResponse.FVsn,
                                                                                      Services = WalletProvider.PollingResponse.Data.Services.ToList(),
                                                                                      ExpiresAt = WalletProvider.PollingResponse.Data.Expires 
                                                                                  };
                                                                   
                                                                   _currentUser.SetWalletProvider(WalletProvider);
                                                                   Debug.Log($"currentUser service count: {_currentUser.Services.Count}");
                                                                   break;
                                                               case PollingStatusEnum.DECLINED:
                                                                   _currentUser = new CurrentUser
                                                                                  {
                                                                                       LoggedIn = false, 
                                                                                       F_type = "USER",
                                                                                       F_vsn = WalletProvider.PollingResponse.FVsn,
                                                                                       ExpiresAt = WalletProvider.PollingResponse.Data.Expires
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
        
        /// <summary>
        /// Logs out the current user and sets the values on the current user object to null.
        /// </summary>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void UnAuthenticate(Action callback = null)
        {
            this._currentUser = null;
            callback?.Invoke();
        }
    }
}