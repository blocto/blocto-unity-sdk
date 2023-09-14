using System;
using System.Collections;
using System.Collections.Generic;
using Blocto.Sdk.Evm;
using Blocto.Sdk.Evm.Model;

namespace Editor.EditTests.Mock
{
    public class MockEthBloctoProvider : BloctoWalletProvider
    {
        public bool ExpectedIsInstallApp { get; set; }

        public bool IsInstallApp { get; set; }

        public bool ForceUseWebSDK { get; set; }

        public Guid MockRequestId { get; set; }

        public Guid MockBloctoAppIdentifier { get; set; }

        public string MockWalletAddress { get; set; }
        
        public object Response { get; set; }
        
        public string ActualUrl { get; set; }

        public string ActualSession { get; set; }

        public string ActualAddress { get; set; }
        
        public Dictionary<string, string> MockRequestIdActionMapper { get; set; }
        
        public void SetUp(Action action)
        {
            RequestIdActionMapper = MockRequestIdActionMapper;
            
            action.Invoke();
            ConnectedWalletAddress = MockWalletAddress;
            BloctoAppIdentifier = MockBloctoAppIdentifier;
            RequestId = MockRequestId;
            IsInstalledApp = IsInstallApp;
            ForceUseWebView = ForceUseWebSDK;
        }
        
        public new void RequestAccount(Action<string> callback)
        {
            base.RequestAccount(callback);
        }

        public new void CreateSignMessageUrl(string message, SignTypeEnum signTypeEnum)
        {
            ActualUrl = base.CreateSignMessageUrl(message, signTypeEnum);
        }
        
        public void SetConnectWalletCallBack(Action<string> callback)
        {
            ConnectWalletCallback = callback;
        }

        public void SetSignMessageCallBack(Action<string> callback)
        {
            SignMessageCallback = callback;
        }

        public string GetSessionId()
        {
            return SessionId;
        }
        
        public string GetAddress()
        {
            return ConnectedWalletAddress;
        }

        public void CreateUrl()
        {
            ActualUrl = base.CreateConnectWalletUrl();
        }

        protected override IEnumerator OpenUrl(string url)
        {
            ActualUrl = url;
            yield break;
        }

        protected override Guid CreateGuid()
        {
            return RequestId;
        }

        protected override TResponse SendData<TPayload, TResponse>(string chain, Func<string, string> getUrl, TPayload requestPayload) where TPayload : class where TResponse : class
        {
            return (TResponse) Response;
        }

        protected void GeneralUrl(string domain, Dictionary<string, string> parameters)
        {
            base.GenerateUrl(domain, parameters);
        }
    }
}