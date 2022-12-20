using System;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Ethereum;
using Blocto.Sdk.Ethereum.Model.Eth;
using UnityEngine;
using UnityEngine.UI;

public class EthController : MonoBehaviour
{
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private Button _connectWalletBtn;
    
    private Button _signMessageBtn;
    
    private Button _sendTransaction;
    
    private Button _testBtn;
    
    private InputField _accountTxt;
    
    private InputField _signMessageTxt;
    
    private Toggle _forceUseWebViewToggle;
    
    private const string _originMessage = "blocto demo app.";
    
    private string _walletAddreass;

    public void Awake()
    {
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("SignBtn");
        _testBtn = tmp.GetComponent<Button>();
        _testBtn.onClick.AddListener(SignMessage);
        
        tmp = GameObject.Find("TestBtn");
        _testBtn = tmp.GetComponent<Button>();
        _testBtn.onClick.AddListener(TestEvent);
        
        tmp = GameObject.Find("AccountTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signMessageTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
        
        _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
                gameObject: gameObject,
                env: "testnet",
                bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
            );
    }


    private void ConnectWallet()
    {
        $"Connect wallet.".ToLog();
        _bloctoWalletProvider.RequestAccount(address => {
                                                _walletAddreass = address;
                                                _accountTxt.text = address;
                                            });
    }
    
    private void SignMessage()
    {
        _bloctoWalletProvider.SignMessage(EthController._originMessage, SignTypeEnum.Personal_Sign, _walletAddreass, signature => {
                                                                                                                         $"Signature: {signature}".ToLog();
                                                                                                                         _signMessageTxt.text = signature;
                                                                                                                     });
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        _bloctoWalletProvider.ForceUseWebView = value;
    }
    
    private void TestEvent()
    {
        var address = "0xFdE70b0920d17f3Ac8fe2Fbb5C979014CD41206A";
        var to = "0x9E168ADDE6654E977C7C22Cc6057ac31b";
        _bloctoWalletProvider.SendTransaction(address, to, 0.01m, "", txId => {
                                                                                     $"TxId: {txId}".ToLog();
                                                                                 });
        // _bloctoWalletProvider.SignMessage("I am Jamis", SignTypeEnum.Personal_Sign, address, signature => {
        //                                                                                     $"Signature: {signature}".ToLog();
        //                                                                                 });
    }
}
