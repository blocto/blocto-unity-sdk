using System;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Models;
using UnityEngine;

namespace Flow.FCL.WalletProvider
{
    public interface IWalletProvider
    {
        public PollingResponse PollingResponse { get; set; }
        
        public AuthzResponse AuthzResponse { get; set; }

        public PreAuthzResponse PreAuthzResponse { get; set; }
        
        public void Login(string authnUrl, Uri pollingUri, Action internalCallback, Action callback = null);
        public void Authz(string iframeUrl, Uri updateUri, Action internalCallback, Action callback = null);
        public void SignMessage(string iframeUrl, string pollingUrl, Action internalCallback = null);
        public string SendTransaction(FlowTransaction transaction, Action intervalCallback);
    }
}