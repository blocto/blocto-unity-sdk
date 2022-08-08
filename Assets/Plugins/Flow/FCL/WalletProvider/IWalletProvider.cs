using System;
using Flow.FCL.Models;
using UnityEngine;

namespace Flow.FCL.WalletProvider
{
    public interface IWalletProvider
    {
        public PollingResponse PollingResponse { get; set; }
        public void Init(GameObject gameObject);
        public void Login(string authnUrl, string pollingUrl, Action internalCallback, Action callback = null); 
        public void SignMessage(string iframeUrl, string pollingUrl, Action internalCallback = null);
    }
}