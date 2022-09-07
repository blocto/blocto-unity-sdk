using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Utility;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using KeyGenerator = Blocto.Sdk.Core.Utility.KeyGenerator;

namespace Flow.FCL.Models
{
    public class CurrentUser : User
    {
        public CurrentUser(IWalletProvider walletProvider, IWebRequestUtils webRequestUtils, IResolveUtility resolveUtility, AppUtils appUtils)
        {
            LoggedIn = false;
            Services = new List<FclService>();
            _walletProvider = walletProvider;
            _webRequestUtils = webRequestUtils;
            _resolveUtility = resolveUtility;
            _appUtils = appUtils;
            _appIdentifier = FlowClientLibrary.Config.Get("appIdentifier");
        }
        
        public List<FclService> Services { get; set; }
        
        private IWalletProvider _walletProvider;
        
        private IWebRequestUtils _webRequestUtils;
        
        private IFlowClient _flowClient;
        
        private IResolveUtility _resolveUtility;
        
        private AppUtils _appUtils;
        
        private string _appIdentifier;
        
        /// <summary>
        /// Returns the current user object.
        /// </summary>
        /// <returns>CurrentUser</returns>
        public CurrentUser Snapshot()
        {
            return this;
        }
        
        /// <summary>
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="url">Authn url</param>
        /// <param name="internalCallback">internal callback</param>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(string url, Action<CurrentUser> callback = null)
        {
            var nonce = KeyGenerator.GetUniqueKey(33).ToLower();
            var parameters = new Dictionary<string, object>
                             {
                                 { "accountProofIdentifier", _appIdentifier },
                                 { "accountProofNonce", nonce.StringToHex() }
                             };
            
            var authnResponse = _webRequestUtils.GetResponse<InitResponse>(url, "POST", "application/json", parameters);
            var authnUrlBuilder = new StringBuilder();
            authnUrlBuilder.Append(authnResponse.Local.Endpoint + "?")
                           .Append(Uri.EscapeDataString("channel") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.Channel) + "&")
                           .Append(Uri.EscapeDataString("appId") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.AppId) + "&")
                           .Append(Uri.EscapeDataString("authenticationId") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.AuthenticationId) + "&")
                           .Append(Uri.EscapeDataString("fclVersion") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.FclVersion) + "&")
                           .Append(Uri.EscapeDataString("accountProofIdentifier") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.AccountProofIdentifier) + "&")
                           .Append(Uri.EscapeDataString("accountProofNonce") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.Local.Params.AccountProofNonce));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(authnResponse.Updates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("authenticationId") + "=")
                             .Append(Uri.EscapeDataString(authnResponse.Updates.Params.AuthenticationId));
            var authnUrl = authnUrlBuilder.ToString();
            var pollingUrl = pollingUrlBuilder.ToString();
            var pollingUri = new Uri(pollingUrl.ToString());
            
            _walletProvider.Login(authnUrl, pollingUri, () => {
                                               switch (_walletProvider.PollingResponse.Status)
                                               {
                                                   case PollingStatusEnum.APPROVED:
                                                       Addr = new FlowAddress(_walletProvider.PollingResponse.Data.Addr); 
                                                       LoggedIn = true;
                                                       F_type = "USER";
                                                       F_vsn = _walletProvider.PollingResponse.FVsn;
                                                       Services = _walletProvider.PollingResponse.Data.Services.ToList();
                                                       ExpiresAt = _walletProvider.PollingResponse.Data.Expires;
                                                       break;
                                                   case PollingStatusEnum.DECLINED:
                                                       LoggedIn = false;
                                                       F_type = "USER";
                                                       F_vsn = _walletProvider.PollingResponse.FVsn;
                                                       break;
                                                   case PollingStatusEnum.PENDING:
                                                   case PollingStatusEnum.REDIRECT:
                                                   case PollingStatusEnum.NONE:
                                                   default:
                                                       break;
                                               }
                                               
                                               var service = Services.FirstOrDefault(service => service.Type == ServiceTypeEnum.AccountProof);
                                               var nonce = service?.Data.Nonce;
                                               var address = service?.Data.Address;
                                               
                                               var isLegal = _appUtils.VerifyAccountProofSignature(_appIdentifier, address, nonce, service?.Data.Signatures.First().SignatureStr);
                                               if(!isLegal)
                                               {
                                                   throw new Exception("Account proof failed");
                                               }
                                               
                                               callback?.Invoke(this);
                                           });
        } 
        
        public PreAuthzResponse PreAuth(FlowTransaction tx, FclService service, Action internalCallback = null)
        {
            var preAuthzUrlBuilder = new StringBuilder();
            preAuthzUrlBuilder.Append(service.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(service.PollingParams.SessionId));
            
            var lastBlock = _flowClient.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ReferenceBlockId = lastBlock.Header.Id;
            
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = _webRequestUtils.GetResponse<PreAuthzResponse>(preAuthzUrlBuilder.ToString(), "POST", "application/json", preSignableJObj);
            
            return preAuthzResponse;
        }
        
        public void SignUserMessage(string message, Action<SignMessageResponse> callback = null)
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
            
            var hexMessage = message.StringToHex();
            var payload = _resolveUtility.ResolveSignMessage(hexMessage, signService.PollingParams.SessionId);
            $"Hex message: {hexMessage}".ToLog();
            $"Payload: {JsonConvert.SerializeObject(payload)}".ToLog();
            signUrl.ToLog();
            
            var response = _webRequestUtils.GetResponse<InitResponse>(signUrl, "POST", "application/json", payload);
            $"Response: {JsonConvert.SerializeObject(response)}".ToLog();
            
            var iframeUrlBuilder = new StringBuilder();
            iframeUrlBuilder.Append(response.Local.Endpoint + "?")
                            .Append(Uri.EscapeDataString("channel") + "=")
                            .Append(Uri.EscapeDataString(response.Local.Params.Channel) + "&")
                            .Append(Uri.EscapeDataString("signatureId") + "=")
                            .Append(Uri.EscapeDataString(response.Local.Params.SignatureId));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(response.Updates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("signatureId") + "=")
                             .Append(Uri.EscapeDataString(response.Updates.Params.SignatureId) + "&")
                             .Append(Uri.EscapeDataString("sessionId") + "=") 
                             .Append(Uri.EscapeDataString(response.Updates.Params.SessionId));
            
            var iframeUrl = iframeUrlBuilder.ToString();
            var pollingUrl =pollingUrlBuilder.ToString();
            var pollingUri = new Uri(pollingUrlBuilder.ToString());
            Debug.Log($"iframe url: {iframeUrl}, polling url: {pollingUrl}");
            
            _walletProvider.SignMessage(iframeUrl, pollingUri, () => {
                                                                   callback.Invoke(_walletProvider.SignMessageResponse);
                                                               });
        }
    }
}