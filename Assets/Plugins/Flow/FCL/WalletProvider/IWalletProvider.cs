using System;

namespace Flow.FCL.WalletProvider
{
    public interface IWalletProvider
    {
        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param> 
        public void Login(string iframeUrl, Uri pollingUri, Action<object> internalCallback);
        
        /// <summary>
        /// Get authorizer signature
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param> 
        public void Authz(string iframeUrl, Uri pollingUri, Action<object> internalCallback);
        
        /// <summary>
        /// SignMessage
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void SignMessage(string iframeUrl, Uri pollingUri, Action<object> internalCallback);
    }
}