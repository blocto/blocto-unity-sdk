using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flow.FCL.Models;
using Flow.FCL.Models.Authn;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.FCL.WalletProvider;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace Flow.FCL
{
    public class CoreModule
    {
        private IWalletProvider _walletProvider;
        
        private IWebRequestUtils _webRequestUtils;
        
        private IResolveUtils _resolveUtils;
        
        private IFlowClient _flowClient;
        
        private string _testScript = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";
        
        private AuthnParams _authnParams;

        public CoreModule(IWalletProvider walletProvider, IFlowClient flowClient, IResolveUtils resolveUtils, UtilFactory utilFactory)
        {
            _walletProvider = walletProvider;
            _flowClient = flowClient;
            _resolveUtils = resolveUtils;
            _webRequestUtils = utilFactory.CreateWebRequestUtil();
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
        public void Authenticate(string url, Action internalCallback, Action callback = null)
        {
            Debug.Log($"Get AuthnResponse, url: {url}");
            var parameters = new Dictionary<string, object>
                             {
                                 { "accountProofIdentifier", "jamisdeapp"},
                                 { "accountProofNonce", KeyGenerator.GetUniqueKey(33).StringToHex() }
                             };
            
            var authnResponse = _webRequestUtils.GetResponse<AuthnResponse>(url, "POST", "application/json", parameters);
            _authnParams = authnResponse.AuthnLocal.Params;
            
            var jsonStr = JsonConvert.SerializeObject(parameters);
            Debug.Log(jsonStr);
            
            var authnUrlBuilder = new StringBuilder();
            authnUrlBuilder.Append(authnResponse.AuthnLocal.Endpoint + "?")
                           .Append(Uri.EscapeDataString("channel") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.Channel) + "&")
                           .Append(Uri.EscapeDataString("appId") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.AppId) + "&")
                           .Append(Uri.EscapeDataString("authenticationId") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.AuthenticationId) + "&")
                           .Append(Uri.EscapeDataString("fclVersion") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.FclVersion) + "&")
                           .Append(Uri.EscapeDataString("accountProofIdentifier") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.AccountProofIdentifier) + "&")
                           .Append(Uri.EscapeDataString("accountProofNonce") + "=")
                           .Append(Uri.EscapeDataString(authnResponse.AuthnLocal.Params.AccountProofNonce));
            
            var pollingUrlBuilder = new StringBuilder();
            pollingUrlBuilder.Append(authnResponse.AuthnUpdates.Endpoint + "?")
                             .Append(Uri.EscapeDataString("authenticationId") + "=")
                             .Append(Uri.EscapeDataString(authnResponse.AuthnUpdates.Params.AuthenticationId));
            var authnUrl = authnUrlBuilder.ToString();
            var pollingUrl = pollingUrlBuilder.ToString();
            var pollingUri = new Uri(pollingUrl.ToString());
            _walletProvider.Login(authnUrl, pollingUri, internalCallback, callback);
        } 
        
        public void PreAuthz(PreSignable preSignable, FclService preAuthzService, Action internalCallback, Action callback = null)
        {
            var preAuthzUrlBuilder = new StringBuilder();
            preAuthzUrlBuilder.Append(preAuthzService.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(preAuthzService.PollingParams.SessionId));
            
            var lastBlock = _flowClient.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            preSignable.Voucher.RefBlock = lastBlock.Header.Id;
            preSignable.Interaction.Message.RefBlock = lastBlock.Header.Id;
            
            var jsonStr = JsonConvert.SerializeObject(preSignable);
            $"preSignable json str: {jsonStr}".ToLog();
            $"preSignable cadence: {Encoding.UTF8.GetBytes(preSignable.Cadence).BytesToHex()}".ToLog();
            $"preSignable cadence base64: {Convert.ToBase64String(Encoding.UTF8.GetBytes(preSignable.Cadence))}".ToLog();
            $"preSignable arguments 1 json str: {JsonConvert.SerializeObject(preSignable.Args.First())}, base64: {Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(preSignable.Args.First())))}".ToLog();
            $"preSignable arguments 1 json str: {JsonConvert.SerializeObject(preSignable.Args[1])}, base64: {Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(preSignable.Args[1])))}".ToLog();
            
            var preAuthzResponse = _webRequestUtils.GetResponse<PreAuthzResponse>(preAuthzUrlBuilder.ToString(), "POST", "application/json", preSignable);
            
            var preAuthzJsonStr = JsonConvert.SerializeObject(preAuthzResponse);
            $"PreauthzResponse json str: {preAuthzJsonStr}".ToLog();
            
            var proposalKey = GetProposerKey(preAuthzResponse.PreAuthzData.Proposer.Identity.Address, preAuthzResponse.PreAuthzData.Proposer.Identity.KeyId).ConfigureAwait(false).GetAwaiter().GetResult();
            var proposer = new Account
                           {
                               Addr = preAuthzResponse.PreAuthzData.Proposer.Identity.Address,
                               KeyId = preAuthzResponse.PreAuthzData.Proposer.Identity.KeyId,
                               TempId = $"{preAuthzResponse.PreAuthzData.Proposer.Identity.Address}-{preAuthzResponse.PreAuthzData.Proposer.Identity.KeyId}",
                               SequenceNum = proposalKey.SequenceNum
                           };
            
            var payers = preAuthzResponse.PreAuthzData.Payer
                                         .Select(payer => new Account
                                                          {
                                                              Addr = payer.Identity.Address,
                                                              KeyId = payer.Identity.KeyId,
                                                              TempId = $"{payer.Identity.Address}-{payer.Identity.KeyId}"
                                                          })
                                         .ToList();
            var authorizations = preAuthzResponse.PreAuthzData.Authorization
                                                 .Select(p => new Account
                                                              {
                                                                  Addr = p.Identity.Address,
                                                                  KeyId = p.Identity.KeyId,
                                                                  TempId = $"{p.Identity.Address}-{p.Identity.KeyId}"
                                                              })
                                                 .ToList();

            $"Create Authz Url".ToLog();
            foreach (var authzItem in preAuthzResponse.PreAuthzData.Authorization)
            {
                var authzUrlBuilder = new StringBuilder();
                authzUrlBuilder.Append(authzItem.Endpoint + "?")
                               .Append(Uri.EscapeDataString("sessionId") + "=")
                               .Append(Uri.EscapeDataString(authzItem.Params.SessionId));
                
                var postUrl = authzUrlBuilder.ToString();
                var postUri = new Uri(postUrl);
                
                var signable = _resolveUtils.ResolveAuthorizerSignable(proposer, payers.First(), authorizations);
                $"Signable json str: {JsonConvert.SerializeObject(signable)}".ToLog();
                
                Debug.Log($"postUrl: {postUrl}");
                var authzResponse = _webRequestUtils.GetResponse<AuthzResponse>(postUrl, "POST", "application/json", signable);
                $"Authz response json: {JsonConvert.SerializeObject(authzResponse)}".ToLog();
                
                var authzGetUrlBuilder = new StringBuilder();
                authzGetUrlBuilder.Append(authzResponse.AuthorizationUpdates.Endpoint.AbsoluteUri + "?")
                                   .Append(Uri.EscapeDataString("sessionId") + "=")
                                   .Append(Uri.EscapeDataString(authzResponse.AuthorizationUpdates.Params.SessionId) + "&")
                                   .Append(Uri.EscapeDataString("authorizationId") + "=")
                                   .Append(Uri.EscapeDataString(authzResponse.AuthorizationUpdates.Params.AuthorizationId));
                                    
                var authzGetUri = new Uri(authzGetUrlBuilder.ToString());
                var authzIframeUrl = authzResponse.Local.First().Endpoint.AbsoluteUri;
                _walletProvider.Authz(authzIframeUrl, 
                                      authzGetUri,
                                      () => {
                                                            Debug.Log("In authz callback.");
                                                            var signature =_walletProvider.AuthzResponse.CompositeSignature.GetValue("signature");
                                                            var addr = _walletProvider.AuthzResponse.CompositeSignature.GetValue("addr");
                                                            Debug.Log($"Authorizer addr: {addr}, signature: {signature}");
                                                            var payerPostUrlBuilder = new StringBuilder();
                                                            var item = preAuthzResponse.PreAuthzData.Payer.First();
                                                            payerPostUrlBuilder.Append(item.Endpoint + "?")
                                                                               .Append(Uri.EscapeDataString("sessionId") + "=")
                                                                               .Append(Uri.EscapeDataString(item.Params.SessionId) + "&")
                                                                               .Append(Uri.EscapeDataString("payerId") + "=")
                                                                               .Append(Uri.EscapeDataString(item.Params.PayerId));
                                                            var payerUri = new Uri(payerPostUrlBuilder.ToString());
                                                            var payerSignable = _resolveUtils.ResolvePayerSignable(payers.First(), authorizations.First(), signable, signature?.ToString());
                                                            var payerSignResponse = _webRequestUtils.GetResponse<PayerSignResponse>(payerUri.AbsoluteUri, "POST", "application/json", payerSignable);
                                                            
                                                            var tx = new FlowTransaction
                                                                     {
                                                                         Script = payerSignable.Cadence,
                                                                         GasLimit = Convert.ToUInt64(payerSignable.Interaction.Message.ComputeLimit),
                                                                         Payer = new FlowAddress(payers.First().Addr),
                                                                         ProposalKey = new FlowProposalKey
                                                                                       {
                                                                                           Address = new FlowAddress(signable.Voucher.ProposalKey.Address.ToString()),
                                                                                           KeyId = Convert.ToUInt32(signable.Voucher.ProposalKey.KeyId),
                                                                                           SequenceNumber = Convert.ToUInt64(signable.Voucher.ProposalKey.SequenceNum)
                                                                                       },
                                                                         ReferenceBlockId = signable.Interaction.Message.RefBlock,
                                                                         Arguments = new List<ICadence>
                                                                                     {
                                                                                         new CadenceNumber(CadenceNumberType.UFix64, "7.5"),
                                                                                         new CadenceAddress("068606b2acddc1ca")
                                                                                     }
                                                                     };
                                                            
                                                            tx.Authorizers.Add(new FlowAddress(authorizations.First().Addr));
                                                            tx.SignerList.Add(authorizations.First().Addr, 1);
                                                            tx.PayloadSignatures.Add(
                                                                new FlowSignature
                                                                {
                                                                    Address = new FlowAddress(authorizations.First().Addr),
                                                                    KeyId = Convert.ToUInt32(authorizations.First().KeyId),
                                                                    Signature = signature?.ToString().StringToBytes().ToArray()
                                                                });
                                                            tx.EnvelopeSignatures.Add(new FlowSignature
                                                                                      {
                                                                                          Address = new FlowAddress(payerSignResponse.Data.GetValue("addr")?.ToString()),
                                                                                          KeyId = Convert.ToUInt32(payerSignResponse.Data.GetValue("keyId")),
                                                                                          Signature = payerSignResponse.Data.GetValue("signature")?.ToString().StringToBytes().ToArray()
                                                                                      });
                                                            
                                                            $"Tx cadence: {Encoding.UTF8.GetBytes(tx.Script).BytesToHex()}".ToLog();
                                                            var txResponse = _flowClient.SendTransactionAsync(tx).ConfigureAwait(false).GetAwaiter().GetResult();
                                                            $"TxId: {txResponse.Id}".ToLog();
                                                         }, null);
            }
        }
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _flowClient.GetAccountAtLatestBlockAsync(address);
            return await account;
        }
        
        private async Task<ProposalKey> GetProposerKey(string addr, int keyId)
        {
            $"Get Proposer key from addr: {addr}".ToLog();
            var account = await GetAccount(addr);
            var proposalKey = account.Keys.First(p => p.Index == Convert.ToUInt32(keyId));
            return new ProposalKey
            {
                Address = addr,
                KeyId = keyId,
                SequenceNum = Convert.ToInt32(proposalKey.SequenceNumber)
            };
        }
    }
}