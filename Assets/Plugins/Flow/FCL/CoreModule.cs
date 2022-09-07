using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Utility;
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
using UnityEngine;
using KeyGenerator = Flow.FCL.Utility.KeyGenerator;

namespace Flow.FCL
{
    public class CoreModule
    {
        private IWalletProvider _walletProvider;
        
        private IWebRequestUtils _webRequestUtils;
        
        private IResolveUtility _resolveUtility;
        
        private IFlowClient _flowClient;
        
        private string _testScript = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";
        
        private string _txId;
        
        private AuthnParams _authnParams;
        

        public CoreModule(IWalletProvider walletProvider, IFlowClient flowClient, IResolveUtility resolveUtility, UtilFactory utilFactory)
        {
            _walletProvider = walletProvider;
            _flowClient = flowClient;
            _webRequestUtils = utilFactory.CreateWebRequestUtil();
            _resolveUtility = utilFactory.CreateResolveUtility();
        }
        
        public void SendTransaction(FlowTransaction tx, FclService fclServices, Action internalCallback, Action callback)
        {
            
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
        
        public FlowTransaction SendTransaction(FclService preAuthzService, FlowTransaction tx, Action internalCallback, Action<string> callback = null)
        {
            var preAuthzUrlBuilder = new StringBuilder();
            preAuthzUrlBuilder.Append(preAuthzService.Endpoint.AbsoluteUri + "?")
                              .Append(Uri.EscapeDataString("sessionId") + "=")
                              .Append(Uri.EscapeDataString(preAuthzService.PollingParams.SessionId));
            
            var lastBlock = _flowClient.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ReferenceBlockId = lastBlock.Header.Id;
            
            var preSignableJObj = _resolveUtility.ResolvePreSignable(ref tx);
            var preAuthzResponse = _webRequestUtils.GetResponse<PreAuthzResponse>(preAuthzUrlBuilder.ToString(), "POST", "application/json", preSignableJObj);
            
            var tmpAccount = GetAccount(preAuthzResponse.PreAuthzData.Proposer.Identity.Address).ConfigureAwait(false).GetAwaiter().GetResult();
            tx.ProposalKey = GetProposerKey(tmpAccount, preAuthzResponse.PreAuthzData.Proposer.Identity.KeyId);

            foreach (var authorization in preAuthzResponse.PreAuthzData.Authorization)
            {
                var authzUrlBuilder = new StringBuilder();
                authzUrlBuilder.Append(authorization.Endpoint + "?")
                               .Append(Uri.EscapeDataString("sessionId") + "=")
                               .Append(Uri.EscapeDataString(authorization.Params.SessionId));
                
                var postUrl = authzUrlBuilder.ToString();
                var authorize = new FlowAccount
                                {
                                    Address = new FlowAddress(authorization.Identity.Address),
                                    Keys = new List<FlowAccountKey>
                                           {
                                               new FlowAccountKey
                                               {
                                                   Index = authorization.Identity.KeyId
                                               }
                                           }
                                };
                var signableJObj = _resolveUtility.ResolveSignable(ref tx, preAuthzResponse.PreAuthzData, authorize);
                var authzResponse = _webRequestUtils.GetResponse<AuthzResponse>(postUrl, "POST", "application/json", signableJObj);
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
                                                            var signature =_walletProvider.AuthzResponse.CompositeSignature.GetValue("signature");
                                                            var addr = _walletProvider.AuthzResponse.CompositeSignature.GetValue("addr");
                                                            if(signature != null)
                                                            {
                                                                var payloadStr = string.Join(",", tx.PayloadSignatures.Select(p => p.Address.Address));
                                                                var payloadSignature = tx.PayloadSignatures.First(p => p.Address.Address == addr.ToString().RemoveHexPrefix());
                                                                payloadSignature.Signature = signature?.ToString().StringToBytes().ToArray();
                                                            }
                                                            
                                                            var payerPostUrlBuilder = new StringBuilder();
                                                            var item = preAuthzResponse.PreAuthzData.Payer.First();
                                                            payerPostUrlBuilder.Append(item.Endpoint + "?")
                                                                               .Append(Uri.EscapeDataString("sessionId") + "=")
                                                                               .Append(Uri.EscapeDataString(item.Params.SessionId) + "&")
                                                                               .Append(Uri.EscapeDataString("payerId") + "=")
                                                                               .Append(Uri.EscapeDataString(item.Params.PayerId));
                                                            var payerUri = new Uri(payerPostUrlBuilder.ToString());
                                                            
                                                            var payerSignable = _resolveUtility.ResolvePayerSignable(ref tx, signableJObj);
                                                            var payerSignResponse = _webRequestUtils.GetResponse<PayerSignResponse>(payerUri.AbsoluteUri, "POST", "application/json", payerSignable);
                                                            signature = payerSignResponse.Data.GetValue("signature");
                                                            addr = payerSignResponse.Data.GetValue("addr");
                                                            if(signature != null && addr != null)
                                                            {
                                                                var envelopeSignature = tx.EnvelopeSignatures.First(p => p.Address.Address == addr.ToString().RemoveHexPrefix());
                                                                envelopeSignature.Signature = signature?.ToString().StringToBytes().ToArray(); 
                                                            }
                                                            
                                                            var txResponse = _flowClient.SendTransactionAsync(tx).ConfigureAwait(false).GetAwaiter().GetResult();
                                                            $"TxId: {txResponse.Id}".ToLog();
                                                            callback?.Invoke(txResponse.Id);
                                                         });
            }
            
            return tx;
        }
        
        
        
        public string GetLastTxId()
        {
            return _txId;
        }
        
        public async Task<FlowAccount> GetAccount(string address)
        {
            var account = _flowClient.GetAccountAtLatestBlockAsync(address);
            return await account;
        }
        
        private void Payer(FlowTransaction tx)
        {
            
        }
        
        private async Task<ProposalKey> GetProposerKey(string addr, int keyId)
        {
            $"Get Proposer key from addr: {addr}".ToLog();
            var account = await GetAccount(addr);
            var proposalKey = account.Keys.First(p => p.Index == Convert.ToUInt32(keyId));
            return new ProposalKey
            {
                Address = addr,
                KeyId = Convert.ToUInt32(keyId),
                SequenceNum = Convert.ToUInt64(proposalKey.SequenceNumber)
            };
        }
        
        private FlowProposalKey GetProposerKey(FlowAccount account, uint keyId)
        {
            var proposalKey = account.Keys.First(p => p.Index == keyId);
            return new FlowProposalKey
                   {
                       Address = account.Address,
                       KeyId = keyId,
                       SequenceNumber = proposalKey.SequenceNumber
                   };
        }
        
        
    }
}