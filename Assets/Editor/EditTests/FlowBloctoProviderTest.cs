using System;
using System.Collections.Generic;
using System.Text;
using Blocto.Sdk.Flow.Model;
using Blocto.Sdk.Flow.Utility;
using Editor.EditTests.Mock;
using Flow.FCL.Models.Authn;
using NUnit.Framework;
using ExpectedObjects;
using Flow.FCL.Models;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;
using Unity.Plastic.Newtonsoft.Json;

namespace Editor.EditTests
{
    [TestFixture]
    public class FlowBloctoProviderTest
    {
        private MockFlowBloctoProvider _target; 
        
        private Guid _mockGuid = new Guid("1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f");
        
        private string _connectWalletUniversalLink = 
            "blocato://sdk?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&address=0xea4e7df22a5f7097&" +
            "account_proof[0][address]=0xea4e7df22a5f7097&account_proof[0][key_id]=0&" +
            "account_proof[0][signature]=6678b3e4131f3933bf86ba3328d4c1225683ea93286e410bf0c10c7df3a36f35a64a156ea6144783489be8d35be973d3ebe8ee85c8000ce34fb1b4405b0c1dcf&" +
            "account_proof[1][address]=0xea4e7df22a5f7097&account_proof[1][key_id]=2&" +
            "account_proof[1][signature]=1ed4655152de59b0b51bfc44818c624b1e83f7d728c6720a516b54da81b723ba524a4981c1a3a1d61689eae1543cf2bbe13db62656f1acc09d03b246b697762e";

        private string _signMessageUniversalLink = 
            "blocato://sdk?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&user_signature[0][address]=0xea4e7df22a5f7097&" +
            "user_signature[0][key_id]=0&" +
            "user_signature[0][signature]=ef7e1d5d2cec85c649674c20e4a9050c1661a8a1e4c65eba56e3bf7a6b474c8a69b59b717a591b25e98a954b4264c523ca6e1e297604a0b7fe0df43f7a956123&" +
            "user_signature[1][address]=0xea4e7df22a5f7097&" +
            "user_signature[1][key_id]=2&" +
            "user_signature[1][signature]=bb2ab65440ea86fa49438ceb98841ea30a4e22f5ec39da09f52635e490f538f60c069d4686fb6ee8a99ee1ada7b314480a5454b87e44312ab2fc6bec887d726e";

        private string _sendTransactionUniversalLink =
            "blocato://sdk?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&tx_hash=aed4bc51e7a226892b0751e4cfbac329ca484531c43f0792f8361bb4ec57d0b1";
        
        [SetUp]
        public void SetUp()
        {
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
        
        [Test]
        public void Create_SendTransaction_Url_On_AppSDK()
        {
            var expectedAddr = "0xea4e7df22a5f7097";
            
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>();
            _target.MockWalletAddress = expectedAddr;
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
                _target.MockTitle = "mocktitle";
                _target.MockThumbnail = "mockthumbnail";
                _target.ConnectedWalletAddress = expectedAddr;
            });

            _target.MockFlowAccount = new FlowAccount
            {
                Address = new FlowAddress(expectedAddr),
                Balance = 10000,
                Keys = new List<FlowAccountKey>
                {
                    GenerateFlowAccountKey(0, "396e230303813e4357844e13c0e75584cffef303a0763ed34d527aa4cbca9e3eeeab656e032b15305032a56deb9e8686a0a8f17f4ba9f00393a7295134030cb1", 999, 4),
                    GenerateFlowAccountKey(1, "9d7db24c3633b0afa770cdba7efc68c512d674eab0329ab94757e33c39b443381ed9ca5afbdd70211a7453eb3b4dc9bdc8ac97912bc18bbc07ecbff09bed12cf", 1000, 0),
                    GenerateFlowAccountKey(2, "780d198b03d2186357851e9bc6539bbc2faa1a133986895c689b6ac66c686c47ee991ddf27cd63675f789289e9768ea20ca64c670c1240035837f1bd605706e5", 1, 0)
                }
            };

            FlowAccountKey GenerateFlowAccountKey(uint index, string publicKey, uint weight, uint sequenceNumber)
            {
                return new FlowAccountKey
                {
                    Index = index,
                    PublicKey = publicKey,
                    Weight = weight,
                    SequenceNumber = sequenceNumber,
                    Revoked = false
                };
            }

            _target.Response = new JObject
            {
                new JProperty("address", "f086a545ce3c552d")
            };

            var actualUrl = _target.CreateSendTransactionUrlForAppSdk(new FlowTransaction 
            { 
                Script = FlowController.MutateScript,
                GasLimit = 1000,
                Arguments = new List<ICadence>
                {
                    new CadenceNumber(CadenceNumberType.UFix64, "333.00000000")
                },
                ReferenceBlockId = "0fb9941fd3152fc42dc12d0cba33e0f1d157056fd5975417b159538439369891"
            });

            var expectedUrl = 
                $"https://blocto.app/sdk?app_id={_mockGuid}&request_id={_mockGuid}&blockchain=flow&method=flow_send_transaction&from={expectedAddr}&flow_transaction=f90123f9011eb8b00a20202020696d706f72742056616c7565446170702066726f6d203078356138313433646138303538373430630a0a202020207472616e73616374696f6e2876616c75653a2055466978363429207b0a20202020202020207072657061726528617574686f72697a65723a20417574684163636f756e7429207b0a20202020202020202020202056616c7565446170702e73657456616c75652876616c7565290a20202020202020207d0a202020207de9a87b2274797065223a22554669783634222c2276616c7565223a223333332e3030303030303030227da00fb9941fd3152fc42dc12d0cba33e0f1d157056fd5975417b1595384393698918203e888ea4e7df22a5f7097800488f086a545ce3c552dc988ea4e7df22a5f7097c0c0";
            Assert.AreEqual(expectedUrl, actualUrl);
        }
        
        [Test]
        public void ConnectWallet_AppSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                {_mockGuid.ToString(), "authn"}
            };
            
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualAuthenticateResponse = default(AuthenticateResponse);
            _target.SetConnectWalletCallBack(item =>
            {
                actualAuthenticateResponse = item as AuthenticateResponse;
            });

            
            _target.UniversalLinkCallbackHandler(_connectWalletUniversalLink);

            var expectedAddr = "0xea4e7df22a5f7097";
            var expectedAuthenticate = new AuthenticateResponse
            {
                Data = new AuthenticateData
                {
                    Addr = expectedAddr,
                    Services = new[]
                    {
                        new FclService
                        {
                            Type = ServiceTypeEnum.AccountProof,
                            Data = new FclServiceData
                            {
                                Address = expectedAddr,
                                FVsn = string.Empty,
                                Signatures = new List<JObject>
                                {
                                    new()
                                    {
                                        new JProperty("keyId", "0"),
                                        new JProperty("signature", "6678b3e4131f3933bf86ba3328d4c1225683ea93286e410bf0c10c7df3a36f35a64a156ea6144783489be8d35be973d3ebe8ee85c8000ce34fb1b4405b0c1dcf")
                                    },
                                    new()
                                    {
                                        new JProperty("keyId", "1"),
                                        new JProperty("signature", "1ed4655152de59b0b51bfc44818c624b1e83f7d728c6720a516b54da81b723ba524a4981c1a3a1d61689eae1543cf2bbe13db62656f1acc09d03b246b697762e")
                                    }
                                }
                            }
                        },
                        new FclService
                        {
                            Type = ServiceTypeEnum.USERSIGNATURE,
                            Addr = expectedAddr,
                            Data = new FclServiceData
                            {
                                Address = expectedAddr
                            }
                        },
                        new FclService
                        {
                            Type = ServiceTypeEnum.PREAUTHZ
                        }
                    }
                }
            };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedAuthenticate), JsonConvert.SerializeObject(actualAuthenticateResponse));
            Assert.AreEqual(expectedAddr, _target.ConnectedWalletAddress);
        }

        [Test]
        public void SignMessage_AppSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                { _mockGuid.ToString(), "signmessage" }
            };

            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualExecuteResult = default(ExecuteResult<List<FlowSignature>>);
            _target.SetSignMessageCallBack(item =>
            {
                actualExecuteResult = item;
            });
            
            _target.UniversalLinkCallbackHandler(_signMessageUniversalLink);
            var expectedAddr = "0xea4e7df22a5f7097";
            var expectedSignatures = new List<FlowSignature>
            {
                new FlowSignature
                {
                    KeyId = 0,
                    Address = new FlowAddress(expectedAddr),
                    Signature = Encoding.UTF8.GetBytes(
                        "ef7e1d5d2cec85c649674c20e4a9050c1661a8a1e4c65eba56e3bf7a6b474c8a69b59b717a591b25e98a954b4264c523ca6e1e297604a0b7fe0df43f7a956123")
                },
                new FlowSignature
                {
                    KeyId = 2,
                    Address = new FlowAddress(expectedAddr),
                    Signature = Encoding.UTF8.GetBytes(
                        "bb2ab65440ea86fa49438ceb98841ea30a4e22f5ec39da09f52635e490f538f60c069d4686fb6ee8a99ee1ada7b314480a5454b87e44312ab2fc6bec887d726e")
                }
            };
            
            var expectedResult = new ExecuteResult<List<FlowSignature>>
            {
                Data = expectedSignatures,
                IsSuccessed = true,
                Message = string.Empty
            };
            
            expectedResult.ToExpectedObject().ShouldEqual(actualExecuteResult);
        }

        [Test]
        public void SendTransaction_AppSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockFlowBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                { _mockGuid.ToString(), "transaction" }
            };

            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockGuid = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualTransactionHash = string.Empty;
            _target.SetSendTransactionCallBack(tx =>
            {
                actualTransactionHash = tx;
            });
            
            _target.UniversalLinkCallbackHandler(_sendTransactionUniversalLink);

            var expectedTranactionHash = "aed4bc51e7a226892b0751e4cfbac329ca484531c43f0792f8361bb4ec57d0b1";
            Assert.AreEqual(expectedTranactionHash, actualTransactionHash);
        }
    }
}