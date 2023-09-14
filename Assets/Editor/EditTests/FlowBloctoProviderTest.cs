using System;
using System.Collections.Generic;
using Blocto.Sdk.Flow.Model;
using Blocto.Sdk.Flow.Utility;
using Editor.EditTests.Mock;
using Flow.FCL.Models.Authn;
using NUnit.Framework;
using ExpectedObjects;
using Flow.FCL.Models;
using Newtonsoft.Json.Linq;

namespace Editor.EditTests
{
    [TestFixture]
    public class FlowBloctoProviderTest
    {
        private MockFlowBloctoProvider _target; 
        
        private Guid _mockGuid = new Guid("1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f");
        
        private Dictionary<string, string> _mockRequestIdActionMapper; 
        
        [SetUp]
        public void SetUp()
        {
            _mockRequestIdActionMapper = new Dictionary<string, string>();
        } 
        
        [Test]
        public void Create_Authenticate_Url_On_WebSDK()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = false;
                _target.ForceUseWebSDK = false;
            });

            var url = "https://123.456";
            var nonce = "asdfghjkl";
            var parameters = new Dictionary<string, object> 
                {
                    { "accountProofIdentifier", _mockGuid },
                    { "accountProofNonce",nonce }
                };

            var expectedAuthenationId = "6tmTKRRoXzJEVSK8TD3hE";
            var expectedHost = "https://wallet-v2-dev.blocto.app";
            _target.Response = new AuthnAdapterResponse
            {
                FType = "PollingResponse",
                Status = "PENDING",
                Updates = new Updates
                {
                    FType = "PollingResponse",
                    Type = "back-channel-rpc",
                    Endpoint = new Uri($"{expectedHost}/api/flow/authn"),
                    Params = new AuthnParams
                    {
                        AuthenticationId = expectedAuthenationId,
                    }
                },
                Local = new Local
                {
                    FType = "Service",
                    Type = "local-view",
                    Endpoint = new Uri(
                        $"{expectedHost}/00000000-0000-0000-0000-000000000000/flow/authn"),
                    Method = "VIEW/IFRAME",
                    Params = new AuthnParams
                    {
                        Channel = "back",
                        AuthenticationId = expectedAuthenationId,
                    }
                }
            };
            
            _target.CreateAuthenticateUrlForWebSdk(url, parameters);
            var expectedUrl = $"{expectedHost}/{_mockGuid}/flow/authn?channel=back&authenticationId={expectedAuthenationId}&";
            var expectedPollingUrl = $"{expectedHost}/api/flow/authn?authenticationId={expectedAuthenationId}";
            var expectedConnectWalletPayload = new ConnectWalletPayload
            {
                Nonce = nonce,
                AppIdentifier = _mockGuid,
                Config = new ConnectWalletConfig{ AppId = _mockGuid.ToString() }
            }.ToExpectedObject();
            
            expectedConnectWalletPayload.ShouldEqual(_target.ActualConnectWalletPayload);
            Assert.AreEqual(expectedUrl, _target.EndPoint.Url);
            Assert.AreEqual(expectedPollingUrl, _target.EndPoint.PollingUrl.ToString());
        }

        [Test]
        public void Create_Authenticate_Url_On_AppSDK()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });

            var nonce = "asdfghjkl";
            var parameters = new Dictionary<string, object> 
            {
                { "accountProofIdentifier", "com.blocto.flow.unitydemo" },
                { "accountProofNonce",nonce }
            };
            
            _target.CreateAuthenticateUrlForAppSdk(parameters);
            
            var expectedUrl = $"https://blocto.app/sdk?app_id={_mockGuid}&request_id={_mockGuid}&blockchain=flow&method=authn&flow_app_id=com.blocto.flow.unitydemo&flow_nonce={nonce}";
            Assert.AreEqual(expectedUrl, _target.EndPoint.Url);
        }

        [Test]
        public void Create_SignMessage_Url_On_WebSDK()
        {
           // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = false;
                _target.ForceUseWebSDK = false;
                _target.MockTitle = "MockTitle";
                _target.MockThumbnail = "MockThumbnail";
                _target.ResolveUtility = new ResolveUtility();
            });

            var mockService = new FclService
            {
                FType = "service",
                Type = ServiceTypeEnum.USERSIGNATURE,
                Endpoint = new Uri("https://wallet-v2-dev.blocto.app/api/flow/user-signature"),
                Id = "DGEJohAHIYdN5uIzj8_-B-FFq-ES9bWLHNM2guEHweF",
                PollingParams = new JObject
                {
                    new JProperty("sessionId", "DGEJohAHIYdN5uIzj8_-B-FFq-ES9bWLHNM2guEHweF")
                }
            };

            var expectedAuthenationId = "ieFunxvjZ_-Pk73a6-iGl";
            var expectedSignatureId = "bckE89fa4fUchmvnMtrhu";
            var expectedSessionId = "GssWudTNCKTLSesF-UJjP-pLC1K998m8tnz-BpuQfSq";
            var expectedHost = "https://wallet-v2-dev.blocto.app";
            _target.Response = new AuthnAdapterResponse
            {
                FType = "PollingResponse",
                Status = "PENDING",
                Updates = new Updates
                {
                    FType = "PollingResponse",
                    Type = "back-channel-rpc",
                    Endpoint = new Uri($"{expectedHost}/api/flow/user-signature"),
                    Params = new AuthnParams
                    {
                        AuthenticationId = expectedAuthenationId,
                        SignatureId = expectedSignatureId,
                        SessionId = expectedSessionId
                    }
                },
                Local = new Local
                {
                    FType = "Service",
                    Type = "local-view",
                    Endpoint = new Uri(
                        $"{expectedHost}/{_mockGuid}/flow/user-signature/{expectedSignatureId}"),
                    Method = "VIEW/IFRAME",
                    Params = new AuthnParams
                    {
                        Channel = "back",
                        AuthenticationId = expectedAuthenationId,
                        SessionId = expectedSessionId
                    }
                }
            };

            var expectedMessage = "test";
            _target.CreateSignMessageUrlForWebSdk(expectedMessage, mockService);

            var expectedUrl =
                $"https://wallet-v2-dev.blocto.app/{_mockGuid}/flow/user-signature/{expectedSignatureId}?channel=back&thumbnail=MockThumbnail&title=MockTitle";
            var expectedPollingUrl =
                $"https://wallet-v2-dev.blocto.app/api/flow/user-signature?signatureId={expectedSignatureId}&sessionId={expectedSessionId}";
            Assert.AreEqual(expectedUrl, _target.EndPoint.Url);
            Assert.AreEqual(expectedPollingUrl, _target.EndPoint.PollingUrl.ToString());
        }

        [Test]
        public void Create_SignMessage_Url_On_AppSDK()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
                _target.MockTitle = "mocktitle";
                _target.MockThumbnail = "mockthumbnail";
            });

            var expectedAddr = "0x123456789";
            var mockService = new FclService
            {
                Addr = expectedAddr
            };
            
            var expectedMessage = "test";
            _target.CreateSignMessageUrlForAppSdk(expectedMessage, mockService);

            var expectedUrl =
                $"https://blocto.app/sdk?app_id={_mockGuid}&request_id={_mockGuid}&blockchain=flow&method=user_signature&from={expectedAddr}&message={expectedMessage}";
            
            Assert.AreEqual(expectedUrl, _target.EndPoint.Url);
        }
    }
}