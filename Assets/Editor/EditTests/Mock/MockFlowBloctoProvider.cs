using System;
using System.Collections.Generic;
using Blocto.Sdk.Flow;
using Blocto.Sdk.Flow.Model;
using Flow.FCL.Models;
using Flow.FCL.Utility;
using Flow.Net.Sdk.Core.Models;

namespace Editor.EditTests.Mock
{
    public class MockFlowBloctoProvider : BloctoWalletProvider
    {
        public bool ExpectedIsInstallApp { get; set; }

        public bool IsInstallApp { get; set; }

        public bool ForceUseWebSDK { get; set; }

        public Guid MockGuid { get; set; }

        public Guid MockBloctoAppIdentifier { get; set; }

        public string MockWalletAddress { get; set; }

        public string MockTitle { get; set; }

        public string MockThumbnail { get; set; }

        public FlowAccount MockFlowAccount { get; set; }
        
        public object Response { get; set; }

        public (string Url, Uri PollingUrl) EndPoint { get; set; }
        
        public string ActualSession { get; set; }

        public string ActualAddress { get; set; }
        
        public IResolveUtility ResolveUtility { get; set; }
        
        public ConnectWalletPayload ActualConnectWalletPayload { get; set; }
        
        public Dictionary<string, string> MockRequestIdActionMapper { get; set; }
        
        public void SetUp(Action action)
        {
            RequestIdActionMapper = MockRequestIdActionMapper;
            
            action.Invoke();
            base.ResolveUtility = ResolveUtility;
            ConnectedWalletAddress = MockWalletAddress;
            BloctoAppIdentifier = MockBloctoAppIdentifier;
            RequestId = MockGuid;
            IsInstalledApp = IsInstallApp;
            ForcedUseWebView = ForceUseWebSDK;
            Title = MockTitle;
            Thumbnail = MockThumbnail;
        }

        public new void CreateAuthenticateUrlForWebSdk(string url, Dictionary<string, object> parameters)
        {
            EndPoint = base.CreateAuthenticateUrlForWebSdk(url, parameters);
        }

        public new void CreateAuthenticateUrlForAppSdk(Dictionary<string, object> parameters)
        {
            EndPoint = base.CreateAuthenticateUrlForAppSdk(parameters);
        }
        
        public new void CreateSignMessageUrlForAppSdk(string message, FclService service)
        {
            var url = base.CreateSignMessageUrlForAppSdk(message, service);
            EndPoint = (url.ToString(), null);
        }

        public new void CreateSignMessageUrlForWebSdk(string message, FclService service)
        {
            EndPoint = base.CreateSignMessageUrlForWebSdk(message, service);
        }
        
        public string CreateSendTransactionUrlForAppSdk(FlowTransaction tx)
        {
            var url = base.CreateSendTransactionUrlForAppSdk(tx);
            return url.ToString();
        }
        
        public void SetConnectWalletCallBack(Action<object> callback)
        {
            AuthenticateCallback = callback;
        }

        public void SetSendTransactionCallBack(Action<string> callback)
        {
            TransactionCallback = callback;
        }

        public void SetSignMessageCallBack(Action<ExecuteResult<List<FlowSignature>>> action)
        {
            SignmessageCallback = action;
        }

        protected override FlowAccount GetFlowAccount()
        {
            return MockFlowAccount;
        }

        protected override ConnectWalletPayload CreateConnectWalletPayload(Dictionary<string, object> parameters)
        {
            ActualConnectWalletPayload = base.CreateConnectWalletPayload(parameters);
            return ActualConnectWalletPayload;
        }

        protected override THttpResponse SendWebRequest<TPayload, THttpResponse>((string Url, string RequestMethod, string ContentType) requestInfo, TPayload payload)
        {
            return (THttpResponse) Response;
        }

        protected override Guid CreateGuid()
        {
            return MockGuid;
        }
    }
}