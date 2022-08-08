using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
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
        
        public void SetWalletProvider(IWalletProvider walletProvider)
        {
            _walletProvider = walletProvider;
        }
        
        public void SetWebRequestHelper(IWebRequestUtils webRequestUtils)
        {
            _webRequestUtils = webRequestUtils;
        }
        
        /// <summary>
        /// Returns the current user object.
        /// </summary>
        /// <returns>CurrentUser</returns>
        public CurrentUser Snapshot()
        {
            return this;
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
            var authnResponse = _webRequestUtils.GetResponse<AuthnResponse>(signUrl);
            
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