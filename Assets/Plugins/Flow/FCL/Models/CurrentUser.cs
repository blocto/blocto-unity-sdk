using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Flow;
using Flow.FCL.Extension;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using KeyGenerator = Blocto.Sdk.Core.Utility.KeyGenerator;

namespace Flow.FCL.Models
{
    public class CurrentUser : User
    {
        public CurrentUser(IWalletProvider walletProvider, IWebRequestUtils webRequestUtils, IResolveUtil resolveUtility, IFlowClient flowClient, AppUtils appUtils)
        {
            LoggedIn = false;
            Services = new List<FclService>();
            _walletProvider = walletProvider;
            _webRequestUtils = webRequestUtils;
            _resolveUtility = resolveUtility;
            _appUtils = appUtils;
            _flowClient = flowClient;
            _appIdentifier = FlowClientLibrary.Config.Get("appIdentifier");
        }
        
        public List<FclService> Services { get; set; }
        
        private IWalletProvider _walletProvider;
        
        private IWebRequestUtils _webRequestUtils;
        
        private IFlowClient _flowClient;
        
        private IResolveUtil _resolveUtility;
        
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
        public void Authenticate(string url, Action<CurrentUser, FlowAccount> callback = null)
        {
            var nonce = KeyGenerator.GetUniqueKey(33).ToLower();
            var parameters = new Dictionary<string, object>
                             {
                                 { "accountProofIdentifier", _appIdentifier },
                                 { "accountProofNonce", nonce.StringToHex() }
                             };
            
            var authnResponse = _webRequestUtils.GetResponse<Authn.AuthnAdapterResponse>(url, "POST", "application/json", parameters);
            var endpoint = authnResponse.AuthnEndpoint();
            
            _walletProvider.Login(endpoint.IframeUrl, endpoint.PollingUrl, item => {
                                               var response = item as AuthenticateResponse;
                                               switch (response?.ResponseStatus)
                                               {
                                                   case ResponseStatusEnum.APPROVED:
                                                       Addr = new FlowAddress(response.Data.Addr); 
                                                       LoggedIn = true;
                                                       F_type = "USER";
                                                       F_vsn = response.FVsn;
                                                       Services = response.Data.Services.ToList();
                                                       ExpiresAt = response.Data.Expires;
                                                       ExpiresAt = response.Data.Expires;
                                                       break;
                                                   case ResponseStatusEnum.DECLINED:
                                                       LoggedIn = false;
                                                       F_type = "USER";
                                                       F_vsn = response.FVsn;
                                                       break;
                                                   case ResponseStatusEnum.PENDING:
                                                   case ResponseStatusEnum.REDIRECT:
                                                   case ResponseStatusEnum.NONE:
                                                   default:
                                                       break;
                                               }
                                               
                                               var service = Services.FirstOrDefault(service => service.Type == ServiceTypeEnum.AccountProof);
                                               var nonce = service?.Data.Nonce;
                                               var address = service?.Data.Address;
                                               var keyId = service?.Data.Signatures.First().KeyId();
                                               
                                               var isLegal = _appUtils.VerifyAccountProofSignature(_appIdentifier, address, keyId, nonce, service?.Data.Signatures.First().SignatureStr());
                                               if(!isLegal)
                                               {
                                                   throw new Exception("Account proof failed");
                                               }
                                               
                                               var account = _flowClient.GetAccountAtLatestBlockAsync(this.Addr.Address).ConfigureAwait(false).GetAwaiter().GetResult();
                                               callback?.Invoke(this, account);
                                           });
        } 
        
        public PreAuthzAdapterResponse PreAuth(FlowTransaction tx, FclService service, Action internalCallback = null)
        {
            var lastBlock = _flowClient.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ReferenceBlockId = lastBlock.Header.Id;
            
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = _webRequestUtils.GetResponse<PreAuthzAdapterResponse>(service.PreAuthAdapterEndpoint(), "POST", "application/json", preSignableJObj);
            
            return preAuthzResponse;
        }
        
        public void SignUserMessage(string message, Action<ExecuteResult<List<(string Source, string Signature, ulong KeyId)>>> callback = null)
        {
            if(Services.All(service => service.Type != ServiceTypeEnum.USERSIGNATURE))
            {
                throw new Exception("Please connect wallet first.");
            }
            
            var signService = Services.First(p => p.Type == ServiceTypeEnum.USERSIGNATURE);
            var signUrl = signService.SignMessageAdapterEndpoint();
            
            var hexMessage = message.StringToHex();
            var payload = _resolveUtility.ResolveSignMessage(hexMessage, signService.PollingParams.SessionId());
            var response = _webRequestUtils.GetResponse<Authn.AuthnAdapterResponse>(signUrl, "POST", "application/json", payload);
            
            var endpoint = response.SignMessageEndpoint();
            _walletProvider.SignMessage(endpoint.IframeUrl, endpoint.PollingUrl, item => {
                                                                                     var response = item as SignMessageResponse;
                                                                                     var signature = response?.Data.First().GetValue("signature")?.ToString();
                                                                                     var keyId = Convert.ToInt32(response?.Data.First().GetValue("keyId")?.ToString());
                                                                                     var result = new ExecuteResult<List<(string Source, string Signature, ulong KeyId)>>
                                                                                                  {
                                                                                                      Data = new List<(string Source, string Signature, ulong KeyId)>
                                                                                                             {
                                                                                                                 (message, signature, Convert.ToUInt64(keyId))
                                                                                                             },
                                                                                                      IsSuccessed = true,
                                                                                                      Message = string.Empty
                                                                                                  };
                                                                                     
                                                                                     callback?.Invoke(result);
                                                                                 });
        }
    }
}