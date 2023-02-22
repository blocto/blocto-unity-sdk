using System;
using System.Collections.Generic;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.Net.Sdk.Core.Models;

namespace Flow.FCL.WalletProvider
{
    public interface IWalletProvider
    {
        /// <summary>
        /// User connect wallet get account
        /// </summary>
        /// <param name="url">fcl authn url</param>
        /// <param name="parameters">parameter of authn</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param>
        public void Authenticate(string url, Dictionary<string, object> parameters, Action<object> internalCallback = null);
        
        /// <summary>
        /// User disconnect wallet
        /// </summary>
        public void UnAuthenticate();
        
        /// <summary>
        /// Send Transaction
        /// </summary>
        /// <param name="service">fcl preauth service</param>
        /// <param name="tx">flow transaction</param>
        /// <param name="internalCallback">completed transaction callback</param>
        public void SendTransaction(FclService service, FlowTransaction tx, Action<string> internalCallback = null);
        
        /// <summary>
        /// Get authorizer signature
        /// </summary>
        /// <param name="iframeUrl">User approve page</param>
        /// <param name="pollingUri">Service endpoint url</param>
        /// <param name="internalCallback">After, get endpoint response internal callback.</param> 
        public void Authz<TResponse>(string iframeUrl, Uri pollingUri, Action<TResponse> internalCallback) where TResponse : IResponse;
        
        /// <summary>
        /// SignMessage
        /// </summary>
        /// <param name="message">Original message </param>
        /// <param name="signService">FCL signature service</param>
        /// <param name="callback">After, get endpoint response callback.</param>
        public void SignMessage(string message, FclService signService, Action<ExecuteResult<List<FlowSignature>>> callback = null);
    }
}