using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm;
using Blocto.Sdk.Evm.Model;
using Blocto.Sdk.Evm.Model.Eth;
using Blocto.Sdk.Evm.Utility;
using Nethereum.ABI;
using Nethereum.Util;
using Nethereum.Web3;
using UnityEngine;
using UnityEngine.UI;

public class EvmController : MonoBehaviour
{
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private Button _connectWalletBtn;
    
    private Button _signMessageBtn;
    
    private Button _sendTransactionBtn;
    
    private Button _transferBtn;
    
    private Button _getValueBtn;
    
    private Button _setValueBtn;
    
    private Button _transferOpenExplorerBtn;
    
    private Button _setValueOpenExplorerBtn;
    
    private InputField _accountTxt;
    
    private InputField _signMessageTxt;
    
    private InputField _receptionAddressTxt;
    
    private InputField _transferValueTxt;
    
    private InputField _transferResultTxt;
    
    private InputField _valueResultTxt;
    
    private InputField _setValueTxt;
    
    private InputField _setValueResultTxt;
    
    private Toggle _forceUseWebViewToggle;
    
    private Dropdown _envDL;
    
    private Dropdown _chainDL;
    
    private WebRequestUtility _webRequestUtility;
    
    private const string _originMessage = "blocto demo app.";
    
    private string _walletAddress;
    
    private Dictionary<int, BloctoWalletProvider> _walletProviderDict;
    
    private List<string> _chains;
    
    public void Awake()
    {
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("SignBtn");
        _signMessageBtn = tmp.GetComponent<Button>();
        _signMessageBtn.onClick.AddListener(SignMessage);
        
        tmp = GameObject.Find("TransferBtn");
        _transferBtn = tmp.GetComponent<Button>();
        _transferBtn.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("GetValueBtn");
        _getValueBtn = tmp.GetComponent<Button>();
        _getValueBtn.onClick.AddListener(GetValue);
        
        tmp = GameObject.Find("SetValueBtn");
        _setValueBtn = tmp.GetComponent<Button>();
        _setValueBtn.onClick.AddListener(SetValue);
        
        tmp = GameObject.Find("TransferOpenExplorerBtn");
        _transferOpenExplorerBtn = tmp.GetComponent<Button>();
        _transferOpenExplorerBtn.onClick.AddListener(TransferOpenExplorer);
        
        tmp = GameObject.Find("SetValueOpenExplorerBtn");
        _setValueOpenExplorerBtn = tmp.GetComponent<Button>();
        _setValueOpenExplorerBtn.onClick.AddListener(SetValueOpenExplorer);
        
        tmp = GameObject.Find("WalletTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signMessageTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _receptionAddressTxt = tmp.GetComponent<InputField>();
        _receptionAddressTxt.text = "0xbF721ABA214E36b710d7C367F4a34BF0f3acdb2D";
        
        tmp = GameObject.Find("TransferValueTxt");
        _transferValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("TransferResultTxt");
        _transferResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ValueResultTxt");
        _valueResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SetValueTxt");
        _setValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SetValueResultTxt");
        _setValueResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("EnvDL");
        _envDL = tmp.GetComponent<Dropdown>();
        _envDL.onValueChanged.AddListener(EnvChanged);
        
        tmp = GameObject.Find("ChainDL");
        _chainDL = tmp.GetComponent<Dropdown>();
        _chainDL.onValueChanged.AddListener(ChainChanged);
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
        
        _webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
        _walletProviderDict = new Dictionary<int, BloctoWalletProvider>();
        _chains = new List<string>
                  {
                      "ethereum",
                      "bsc",
                      "polygon",
                      "avalanche"
                  };
    }


    private void ConnectWallet()
    {
        $"Connect wallet.".ToLog();
        var envIndex = _envDL.value;
        if(_walletProviderDict.Keys.Contains(envIndex) == false)
        {
            _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
                gameObject: gameObject,
                env: envIndex == 0 ? "mainnet" : "testnet",
                bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
            );
            
            _walletProviderDict.Add(envIndex, _bloctoWalletProvider);
            
            var chainIndex = _chainDL.value;
            var chainName = (ChainEnum)Enum.Parse()
            _bloctoWalletProvider.Chain = chainName;
        }
        else
        {
            _bloctoWalletProvider = _walletProviderDict[envIndex];
        }
        
        _bloctoWalletProvider.RequestAccount(address => {
                                                 _walletAddress = address;
                                                 _accountTxt.text = address;
                                             });
    }
    
    private void ChainChanged(int index)
    {
        $"Chain index: {index}, ChainDD value: {_chainDL.value}".ToLog();
        var chainIndex = _chainDL.value;
        Enum.TryParse(_chains[chainIndex], out ChainEnum chainName);
        if(_bloctoWalletProvider != null)
        {
            _bloctoWalletProvider.Chain = chainName;
        }
    }
    
    private void EnvChanged(int index)
    {
        $"Env index: {index}".ToLog();
    }
    
    private void SignMessage()
    {
        _bloctoWalletProvider.SignMessage(EvmController._originMessage, SignTypeEnum.Personal_Sign, _walletAddress, signature => {
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
        $"Transfer value: {value}".ToLog();
        _bloctoWalletProvider.SendTransaction(address, receptionAddress, value, "", txId => {
                                                                                        $"TxId: {txId}".ToLog();
                                                                                        _transferResultTxt.text = txId;
                                                                                    }); 
    }
    
    private void TransferOpenExplorer()
    {
        var url = $"https://rinkeby.etherscan.io/tx/{_transferResultTxt.text}";
        Application.OpenURL(url);
    }
    
    private void GetValue()
    {
        _bloctoWalletProvider.NodeUrl = "https://rinkeby.blocto.app";
        var abiUrl = new Uri("https://api-rinkeby.etherscan.io/api?module=contract&action=getabi&address=0x58F385777aa6699b81f741Dd0d5B272A34C1c774");
        var queryResult = _bloctoWalletProvider.QueryForSmartContract<int>(abiUrl, "0x58F385777aa6699b81f741Dd0d5B272A34C1c774", "value");
        _valueResultTxt.text = queryResult.ToString();
    }
    
    private void SetValue()
    {
        var address = _accountTxt.text;
        var contractAddress = "0x58F385777aa6699b81f741Dd0d5B272A34C1c774";
        var abiUrl = new Uri("https://api-rinkeby.etherscan.io/api?module=contract&action=getabi&address=0x58F385777aa6699b81f741Dd0d5B272A34C1c774");
        var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
        var web3 = new Web3("https://rinkeby.blocto.app");
        var contract = web3.Eth.GetContract(api.Result, contractAddress);
        var setValue = contract.GetFunction("setValue");
        var data = setValue.GetData(new object[]{ Convert.ToUInt64(_setValueTxt.text) });
        $"Set Value: {_setValueTxt.text}, encode data: {data}".ToLog();
        _bloctoWalletProvider.SendTransaction(address, contractAddress, 0, data, txId => {
                                                                                     $"TxId: {txId}".ToLog();
                                                                                     _setValueResultTxt.text = txId;
                                                                                 });
    }
    
    private void SetValueOpenExplorer()
    {
        var url = $"https://rinkeby.etherscan.io/tx/{_setValueResultTxt.text}";
        Application.OpenURL(url);
    }
}