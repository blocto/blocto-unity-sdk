using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Extensions;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core.Models;

namespace Flow.FCL.Models
{
    public class CurrentUser : User
    {
        public CurrentUser(IWalletProvider walletProvider)
        {
            LoggedIn = false;
            Services = new List<FclService>();
            _walletProvider = walletProvider;
        }
        
        public List<FclService> Services { get; set; }

        private AccountProofData AccountProofData { get; set; }
        
        private readonly IWalletProvider _walletProvider;
        
        private readonly IWebRequestUtils _webRequestUtils;
        
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
            
            _walletProvider.Authenticate(url, parameters, item => {
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
                                                           foreach (var signature in service?.Data.Signatures)
                                                           {
                                                               accountProofData.Signature.Add(new FlowSignature()
                                                                                              {
                                                                                                  Address = new FlowAddress(service?.Data.Address),
                                                                                                  KeyId = Convert.ToUInt32(signature.KeyId()),
                                                                                                  Signature = Encoding.UTF8.GetBytes(signature.SignatureStr())
                                                                                              });
                                                           }
                                                           
                                                           AccountProofData = accountProofData;
                                                           callback?.Invoke(this, accountProofData);
                                                       }else
                                                       {
                                                           callback?.Invoke(this, null);
                                                       }}) ;
        }
        
        public void SignUserMessage(string message, Action<ExecuteResult<List<FlowSignature>>> callback = null)
        {
            if(Services.All(service => service.Type != ServiceTypeEnum.USERSIGNATURE))
            {
                throw new Exception("Please connect wallet first.");
            }
            
            var signService = Services.First(p => p.Type == ServiceTypeEnum.USERSIGNATURE);
            _walletProvider.SignMessage(message, signService, callback);
        }
    }
}