using System;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Evm;
using Blocto.Sdk.Evm.Model.Eth;
using Blocto.Sdk.Evm.Utility;
using UnityEngine;
using UnityEngine.UI;

public class EvmController : MonoBehaviour
{
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private Button _connectWalletBtn;
    
    private Button _signMessageBtn;
    
    private Button _verifySignMessageBtn;
    
    private Button _sendTransactionBtn;
    
    private Button _getValueBtn;
    
    private Button _testBtn;
    
    private InputField _accountTxt;
    
    private InputField _signMessageTxt;
    
    private InputField _receptionAddressTxt;
    
    private InputField _transferValueTxt;
    
    private InputField _valueResultTxt;
    
    private Toggle _forceUseWebViewToggle;
    
    private Dropdown _envDL;
    
    private Dropdown _chainDL;
    
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
        
        tmp = GameObject.Find("VerifyBtn");
        _verifySignMessageBtn = tmp.GetComponent<Button>();
        _verifySignMessageBtn.onClick.AddListener(SignMessage);
        
        tmp = GameObject.Find("VerifyBtn");
        _sendTransactionBtn = tmp.GetComponent<Button>();
        _sendTransactionBtn.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("TransferBtn");
        _testBtn = tmp.GetComponent<Button>();
        _testBtn.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("GetValueBtn");
        _testBtn = tmp.GetComponent<Button>();
        _testBtn.onClick.AddListener(GetValue);
        
        tmp = GameObject.Find("WalletTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signMessageTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _receptionAddressTxt = tmp.GetComponent<InputField>();
        _receptionAddressTxt.text = "0xbF721ABA214E36b710d7C367F4a34BF0f3acdb2D";
        
        tmp = GameObject.Find("TransferValueTxt");
        _transferValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ValueResultTxt");
        _valueResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("EnvDL");
        _envDL = tmp.GetComponent<Dropdown>();
        _envDL.onValueChanged.AddListener(EnvChanged);
        
        tmp = GameObject.Find("ChainDL");
        _chainDL = tmp.GetComponent<Dropdown>();
        _chainDL.onValueChanged.AddListener(ChainChanged);
        
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
    
    private void ChainChanged(int index)
    {
        $"Chain index: {index}, ChainDD value: {_chainDL.value}".ToLog();
    }
    
    private void EnvChanged(int index)
    {
        $"Env index: {index}".ToLog();
    }
    
    private void SignMessage()
    {
        _bloctoWalletProvider.SignMessage(EvmController._originMessage, SignTypeEnum.Personal_Sign, _walletAddreass, signature => {
                                                                                                                         $"Signature: {signature}".ToLog();
                                                                                                                         _signMessageTxt.text = signature;
                                                                                                                     });
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        _bloctoWalletProvider.ForceUseWebView = value;
    }
    
    private void SendTransaction()
    {
        var address = _accountTxt.text;
        var receptionAddress = _receptionAddressTxt.text;
        var value = Convert.ToDecimal(_transferValueTxt.text);
        _bloctoWalletProvider.SendTransaction(address, receptionAddress, 0.001m, "", txId => {
                                                                                         $"TxId: {txId}".ToLog();
                                                                                     }); 
    }
    
    private void GetValue()
    {
        _bloctoWalletProvider.NodeUrl = "https://rinkeby.blocto.app";
        var abiUrl = new Uri("https://api-rinkeby.etherscan.io/api?module=contract&action=getabi&address=0x58F385777aa6699b81f741Dd0d5B272A34C1c774");
        var queryResult = _bloctoWalletProvider.QueryForSmartContract<int>(abiUrl, "0x58F385777aa6699b81f741Dd0d5B272A34C1c774", "value");
        _valueResultTxt.text = queryResult.ToString();
    }
}