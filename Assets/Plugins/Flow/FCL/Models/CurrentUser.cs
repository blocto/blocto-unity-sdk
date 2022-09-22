using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Extensions;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plugins.Flow.FCL.Models;

namespace Flow.FCL.Models
{
    public class CurrentUser : User
    {
        public CurrentUser(IWalletProvider walletProvider, IWebRequestUtils webRequestUtils, IResolveUtility resolveUtility, IFlowClient flowClient)
        {
            LoggedIn = false;
            Services = new List<FclService>();
            _walletProvider = walletProvider;
            _webRequestUtils = webRequestUtils;
            _resolveUtility = resolveUtility;
            _flowClient = flowClient;
        }
        
        public List<FclService> Services { get; set; }

        private AccountProofData AccountProofData { get; set; }
        
        private readonly IWalletProvider _walletProvider;
        
        private readonly IWebRequestUtils _webRequestUtils;
        
        private readonly IFlowClient _flowClient;
        
        private readonly IResolveUtility _resolveUtility;
        
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
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(string url, Action<CurrentUser, AccountProofData> callback = null)
        {
            Authenticate(url, null, callback);
        } 
        
        /// <summary>
        /// Calling this method will authenticate the current user via any wallet that supports FCL.
        /// Once called, FCL will initiate communication with the configured discovery.wallet endpoint which lets the user select a wallet to authenticate with.
        /// Once the wallet provider has authenticated the user,
        /// FCL will set the values on the current user object for future use and authorization.
        /// </summary>
        /// <param name="url">Authn url</param>
        /// <param name="accountProofData">Flow account proof data</param>
        /// <param name="callback">The callback will be called when the user authenticates and un-authenticates, making it easy to update the UI accordingly.</param>
        public void Authenticate(string url, AccountProofData accountProofData = null, Action<CurrentUser, AccountProofData> callback = null)
        {
            var parameters = new Dictionary<string, object>();
            if(accountProofData != null)
            {
                parameters = new Dictionary<string, object>
                                 {
                                     { "accountProofIdentifier", accountProofData.AppId },
                                     { "accountProofNonce", accountProofData.Nonce }
                                 };
            }
            
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
                                                                               
                                                                              if(accountProofData != null)
                                                                              {
                                                                                  var service = Services.FirstOrDefault(service => service.Type == ServiceTypeEnum.AccountProof);
                                                                                  var nonce = service?.Data.Nonce;
                                                                                  accountProofData.Signature = new Signature
                                                                                                               {
                                                                                                                   Addr = service?.Data.Address,
                                                                                                                   KeyId = Convert.ToUInt32(service?.Data.Signatures.First().KeyId()),
                                                                                                                   SignatureStr = service?.Data.Signatures.First().SignatureStr()
                                                                                                               };
                                                                                  AccountProofData = accountProofData;
                                                                                  callback?.Invoke(this, accountProofData);
                                                                              }else
                                                                              {
                                                                                  callback?.Invoke(this, null);
                                                                              }
                                                                           });
        }
        
        public void SignUserMessage(string message, Action<ExecuteResult<FlowSignature>> callback = null)
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
                                                                                     var signature = response?.Data.First().SignatureStr();
                                                                                     var keyId = Convert.ToUInt32(response?.Data.First().KeyId());
                                                                                     var addr = response?.Data.First().Address();
                                                                                     var result = new ExecuteResult<FlowSignature>
                                                                                                  {
                                                                                                      Data = new FlowSignature
                                                                                                             {
                                                                                                                 Address = new FlowAddress(addr),
                                                                                                                 KeyId = keyId,
                                                                                                                 Signature = Encoding.UTF8.GetBytes(signature!)
                                                                                                             },
                                                                                                      IsSuccessed = true,
                                                                                                      Message = string.Empty
                                                                                                  };
                                                                                     
                                                                                     callback?.Invoke(result);
                                                                                 });
        }
    }
}