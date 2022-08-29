using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Models.Authn;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL.Models
{
    public class CurrentUser : User
    {
        public CurrentUser()
        {
            LoggedIn = false;
            Services = new List<FclService>();
        }
        
        public List<FclService> Services { get; set; }
        
        private IWalletProvider _walletProvider;
        
        private IWebRequestUtils _webRequestUtils;
        
        private CoreModule _coreModule;
        
        private CurrentUser _currentUser;
        
        private Config.Config _config;
        
        public void SetWalletProvider(IWalletProvider walletProvider)
        {
            _walletProvider = walletProvider;
        }
        
        public void SetCoreModule(CoreModule coreModule)
        {
            _coreModule = coreModule;
        }
        
        public void SetWebRequestHelper(IWebRequestUtils webRequestUtils)
        {
            _webRequestUtils = webRequestUtils;
        }
        
        public void SetCurrentUser(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }
        
        public void SetConfig(Config.Config config)
        {
            _config = config;
        }
        
        public string GetLastTxId()
        {
            return _coreModule.GetLastTxId();
        }
        
        /// <summary>
        /// Returns the current user object.
        /// </summary>
        /// <returns>CurrentUser</returns>
        public CurrentUser Snapshot()
        {
            return _currentUser;
        }
        
        public void Authenticate(Action callback = null)
        {
            var url = _config.Get("discovery.wallet");
            _coreModule.Authenticate(url, () => {
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
                                                       
                                                       _currentUser.SetWalletProvider(_walletProvider);
                                                       _currentUser.SetCoreModule(_coreModule);
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
        
        public void SignUserMessage()
        {
            foreach (var service in Services)
            {
                Debug.Log($"Type: {service.Type}, endpoint: {service.Endpoint}");
            }
            
            var signService = Services.First(p => p.Type == ServiceTypeEnum.USERSIGNATURE);
            var signUrlBuilder = new StringBuilder();
            signUrlBuilder.Append(signService.Endpoint + "?")
                   .Append(Uri.EscapeDataString("sessionId") + "=")
                   .Append(Uri.EscapeDataString(signService.PollingParams.SessionId));
            var signUrl = signUrlBuilder.ToString();
            
            signUrl.ToLog();
            var authnResponse = _webRequestUtils.GetResponse<AuthnResponse>(signUrl, "POST", "application/json", new Dictionary<string, object>());
            
            var iframeUrlBuilder = new StringBuilder();
            iframeUrlBuilder.Append(authnResponse.AuthnLocal.Endpoint + "?")
                            .Append(Uri.EscapeDataString("channel") + "=")
                            .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.Channel) + "&")
                            .Append(Uri.EscapeDataString("signatureId") + "=")
                            .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.SignatureId));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(authnResponse.AuthnUpdates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("signatureId") + "=")
                             .Append(Uri.EscapeDataString(authnResponse.AuthnUpdates.Params.SignatureId))
                             .Append(Uri.EscapeDataString("sessionId") + "=") 
                             .Append(Uri.EscapeDataString(authnResponse.AuthnUpdates.Params.SessionId));
            
            var iframeUrl = iframeUrlBuilder.ToString();
            var pollingUrl = pollingUrlBuilder.ToString();
            Debug.Log($"iframe url: {iframeUrl}, polling url: {pollingUrl}");
            
            _walletProvider.SignMessage(iframeUrl, pollingUrl);
        }
    }
}