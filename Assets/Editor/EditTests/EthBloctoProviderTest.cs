using System;
using System.Collections;
using System.Collections.Generic;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Evm;
using Blocto.Sdk.Evm.Model;
using Editor.EditTests.Mock;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace Editor.EditTests
{
    [TestFixture]
    public class EthBloctoProviderTest
    {
        private MockEthBloctoProvider _target;

        private string _mockWalletAddress = "0x987654321";
        
        private Guid _mockGuid = new Guid("1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f");

        private string _connectWalletWebCallBackStr =
            "blocto://?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&address=0xa7140DbaED1ad3BCC032C8d5C9886417cFbA31F9&session_id=735tNjrjLPLAjOeii0ve5-HKelDFSPwkcmZbMR23FxZ";

        private string _signMessageWebCallBackStr =
            "blocto://?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&result=ok&signature=0x411c6d40c0f127bdb5e28e3b1cd42261aeabf4fc6354de529fed4c784dcd45e41b335b71d13bd870a5bf3408e2aa6c063aea1b02233b8c392028b46117c9a32980";

        private Dictionary<string, string> _mockRequestIdActionMapper; 
        
        [SetUp]
        public void SetUp()
        {
            _mockRequestIdActionMapper = new Dictionary<string, string>();
        }

        [Test]
        public void CreateUrl_For_Open_WebSDK_On_RequestAccount()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = false;
                _target.ForceUseWebSDK = false;
            });
            
            _target.CreateUrl();
            var expectedUrl = $"https://wallet-v2.blocto.app/1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f/ethereum/authn/?request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&request_source=sdk_unity";
            Assert.AreEqual(expectedUrl, _target.ActualUrl);
        }
        
        [Test]
        public void CreateUrl_For_Open_AppSDK_On_RequestAccount()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            _target.CreateUrl();
            var expectedUrl = $"https://blocto.app/sdk?app_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&blockchain=ethereum&method=request_account&request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&platform=sdk_unity";
            Assert.AreEqual(expectedUrl, _target.ActualUrl);
        }

        [Test]
        public void ConnectWallet_WebSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                {_mockGuid.ToString(), "CONNECTWALLET"}
            };
            
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualAddress = "";
            _target.SetConnectWalletCallBack(addr =>
            {
                actualAddress = addr;
            });
            
            _target.UniversalLinkCallbackHandler(_connectWalletWebCallBackStr);
            Assert.AreEqual("735tNjrjLPLAjOeii0ve5-HKelDFSPwkcmZbMR23FxZ", _target.GetSessionId());
            Assert.AreEqual("0xa7140DbaED1ad3BCC032C8d5C9886417cFbA31F9", actualAddress);
        }

        [Test]
        public void SignMessage_WebSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                {_mockGuid.ToString(), "SIGNMESSAGE"}
            };
            
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualSignature = "";
            _target.SetSignMessageCallBack(signature =>
            {
                actualSignature = signature;
            });
            
            _target.UniversalLinkCallbackHandler(_signMessageWebCallBackStr);

            var expectedSignature =
                "0x411c6d40c0f127bdb5e28e3b1cd42261aeabf4fc6354de529fed4c784dcd45e41b335b71d13bd870a5bf3408e2aa6c063aea1b02233b8c392028b46117c9a32980";
            Assert.AreEqual(expectedSignature, actualSignature);
        }
        
        [Test]
        public void SendTransaction_WebSDK_UniversalLink_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.MockRequestIdActionMapper = new Dictionary<string, string>
            {
                {_mockGuid.ToString(), "SIGNMESSAGE"}
            };
            
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
            });
            
            var actualSignature = "";
            _target.SetSignMessageCallBack(signature =>
            {
                actualSignature = signature;
            });
            
            _target.UniversalLinkCallbackHandler(_signMessageWebCallBackStr);

            var expectedSignature =
                "0x411c6d40c0f127bdb5e28e3b1cd42261aeabf4fc6354de529fed4c784dcd45e41b335b71d13bd870a5bf3408e2aa6c063aea1b02233b8c392028b46117c9a32980";
            Assert.AreEqual(expectedSignature, actualSignature);
        }
        
        [Test]
        public void CreateSignMessageUrl_For_Open_WebSDK_On_SignMessage_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = false;
                _target.ForceUseWebSDK = false;
                _target.Response = new SignMessagePreResponse
                {
                    Status = "PENDING",
                    SignatureId = "irhKL-07dk_AXEu6oGscU",
                    Reason = ""
                }; 
            });
            
            _target.CreateSignMessageUrl("0x1234567890", SignTypeEnum.Personal_Sign);
            var expectedUrl = $"https://wallet-v2.blocto.app/{_mockGuid}/ethereum/user-signature/irhKL-07dk_AXEu6oGscU";
            Assert.AreEqual(expectedUrl, _target.ActualUrl);
        }
        
        [Test]
        public void CreateSignMessageUrl_For_Open_AppSDK_On_SignMessage_Test()
        {
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            _target = new MockEthBloctoProvider();
            _target.SetUp(() =>
            {
                _target.MockWalletAddress = _mockWalletAddress;
                _target.MockBloctoAppIdentifier = _mockGuid;
                _target.MockRequestId = _mockGuid;
                _target.IsInstallApp = true;
                _target.ForceUseWebSDK = false;
                _target.Response = new SignMessagePreResponse
                {
                    Status = "PENDING",
                    SignatureId = "irhKL-07dk_AXEu6oGscU",
                    Reason = ""
                }; 
            });
            
            _target.CreateSignMessageUrl("0x1234567890", SignTypeEnum.Personal_Sign);
            var expectedUrl = $"https://blocto.app/sdk?app_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&blockchain=ethereum&method=sign_message&from=0x987654321&type=personal_sign&message=0x1234567890&request_id=1dc194b6-2d9e-4b01-b5a8-a8416e1bd18f&platform=sdk_unity";
            Assert.AreEqual(expectedUrl, _target.ActualUrl);
        }
    }
}