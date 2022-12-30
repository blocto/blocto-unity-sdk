using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm;
using Blocto.Sdk.Evm.Model;
using Blocto.Sdk.Evm.Model.Eth;
using Blocto.Sdk.Solana.Model;
using Nethereum.Web3;
using Script.Model;
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
    
    private Button _ethSignBtn;
    
    private Button _personalSignBtn;
    
    private Button _typedDataV3Btn;
    
    private Button _typedDataV4Btn;
    
    private Button _typedDataBtn;
    
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
    
    private ChainInformation _selectedChain;
    
    private SignTypeEnum _signType;
    
    private EthSignSample _ethSignSample;
    
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
        
        tmp = GameObject.Find("EthSignBtn");
        _ethSignBtn = tmp.GetComponent<Button>();
        _ethSignBtn.onClick.AddListener(EthSignSample);
        
        tmp = GameObject.Find("PersonalSignBtn");
        _personalSignBtn = tmp.GetComponent<Button>();
        _personalSignBtn.onClick.AddListener(PersonalSignSample);
        
        tmp = GameObject.Find("TypedDataV3Btn");
        _typedDataV3Btn = tmp.GetComponent<Button>();
        _typedDataV3Btn.onClick.AddListener(TypedDataV3);
        
        tmp = GameObject.Find("TypedDataV4Btn");
        _typedDataV4Btn = tmp.GetComponent<Button>();
        _typedDataV4Btn.onClick.AddListener(TypedDataV4);
        
        tmp = GameObject.Find("TypedDataBtn");
        _typedDataBtn = tmp.GetComponent<Button>();
        _typedDataBtn.onClick.AddListener(TypedData);
        
        tmp = GameObject.Find("WalletTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signMessageTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _receptionAddressTxt = tmp.GetComponent<InputField>();
        _receptionAddressTxt.text = "0xd291Eb0048de837A469B3c1A1E9615F0A7860276";
        
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
        _chainDL.value = 0;
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
        
        _webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
        _walletProviderDict = new Dictionary<int, BloctoWalletProvider>();
        _selectedChain = EvmChain.ETHEREUM;
        _chains = new List<string>
                  {
                      "ethereum",
                      "bsc",
                      "polygon",
                      "avalanche"
                  };
        
        _ethSignSample = new EthSignSample();
        // ReadAppSetting();
    }


    private void ConnectWallet()
    {
        $"Connect wallet.".ToLog();
        var envIndex = _envDL.value;
        if(_walletProviderDict.Keys.Contains(envIndex) == false)
        {
            _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
                gameObject: gameObject,
                env: envIndex == 0 ? EnvEnum.Mainnet : EnvEnum.Devnet,
                bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
            );
            
            _bloctoWalletProvider.ForceUseWebView = _forceUseWebViewToggle.isOn;
            
            var chainIndex = _chainDL.value;
            var chainName = (ChainEnum)Enum.Parse(typeof(ChainEnum), _chains[chainIndex], true);
            _bloctoWalletProvider.Chain = chainName;
            _walletProviderDict.Add(envIndex, _bloctoWalletProvider);
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
        var chain = (ChainEnum)Enum.Parse(typeof(ChainEnum), _chains[chainIndex], true);
        if(_bloctoWalletProvider != null)
        {
            _bloctoWalletProvider.Chain = chain;
        }

        // switch (chain)
        // {
        //     case ChainEnum.Ethereum:
        //         _selectedChain = EvmChain.ETHEREUM;
        //         break;
        //     case ChainEnum.BSC:
        //         _selectedChain = EvmChain.BNB_CHAIN;
        //         break;
        //     case ChainEnum.Polygon:
        //         _selectedChain = EvmChain.POLYGON;
        //         break;
        //     case ChainEnum.Avalanche:
        //         _selectedChain = EvmChain.AVALANCHE;
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }

        _selectedChain = chain switch
                         {
                             ChainEnum.Ethereum => EvmChain.ETHEREUM,
                             ChainEnum.BSC => EvmChain.BNB_CHAIN,
                             ChainEnum.Polygon => EvmChain.POLYGON,
                             ChainEnum.Avalanche => EvmChain.AVALANCHE,
                             _ => throw new ArgumentOutOfRangeException()
                         };
    }
    
    private void EnvChanged(int index)
    {
        $"Env index: {index}".ToLog();
    }
    
    private void SignMessage()
    {
        $"Sign type: {_signType}, message: {_signMessageTxt.text}".ToLog();
        _bloctoWalletProvider.SignMessage(_signMessageTxt.text, _signType, _walletAddress, signature => {
                                                                                                                        $"Signature: {signature}".ToLog();
                                                                                                                        _signMessageTxt.text = signature;
                                                                                                                    });
        var web3 = new Web3(IsMainnet() ? _selectedChain.MainnetRpcUrl : _selectedChain.TestnetRpcUrl);
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        _bloctoWalletProvider.ForceUseWebView = value;
    }
    
    private void SendTransaction()
    {
        var receptionAddress = default(string);
        _accountTxt.text = "0x57FDcA3F5961A8D8715e15b6770311668f3eD4DB";
        var address = _accountTxt.text;
        // var value = Convert.ToUInt64(_transferValueTxt.text) * 1000000000000000000;
        var value = Convert.ToUInt64(_transferValueTxt.text) * 100000000;
        if(_selectedChain.Title == "BNB Chain")
        {
            _receptionAddressTxt.text = "0xd291Eb0048de837A469B3c1A1E9615F0A7860276";
            receptionAddress = EvmChain.BLT.TestnetContractAddress;
            var abiUrl = new Uri($"{_selectedChain.TestnetExplorerApiUrl}/api?module=contract&action=getabi&address={EvmChain.BLT.TestnetContractAddress}");
            var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
            $"ABI json: {api.Result}".ToLog();
        
            var web3 = new Web3(IsMainnet() ? _selectedChain.MainnetRpcUrl : _selectedChain.TestnetRpcUrl);
            var contract = web3.Eth.GetContract(api.Result, IsMainnet() ? EvmChain.BLT.MainnetContractAddress : EvmChain.BLT.TestnetContractAddress);
            var transfer = contract.GetFunction("transfer");
            var data = transfer.GetData(new object[]{ _receptionAddressTxt.text, value });
            $"Reception address: {_receptionAddressTxt.text}, Set Value: {value}, encode data: {data}".ToLog();
            _bloctoWalletProvider.SendTransaction(
                address, 
                receptionAddress, 
                0, 
                data, 
                txId => {
                    $"TxId: {txId}".ToLog();
                    _transferResultTxt.text = txId;
                });
        }
        else
        {
            receptionAddress = _receptionAddressTxt.text;
            $"Transfer value: {value}".ToLog();
            _bloctoWalletProvider.SendTransaction(address, receptionAddress, value, "", txId => {
                                                                                            $"TxId: {txId}".ToLog();
                                                                                            _transferResultTxt.text = txId;
                                                                                        }); 
        }
    }
    
    private void TransferOpenExplorer()
    {
        var url = $"https://{_selectedChain.TestnetExplorerDomain}/tx/{_transferResultTxt.text}";
        Application.OpenURL(url);
    }
    
    private void GetValue()
    {
        var abiUrl = new Uri($"{_selectedChain.TestnetExplorerApiUrl}/api?module=contract&action=getabi&address={_selectedChain.TestnetContractAddress}");
        _bloctoWalletProvider.NodeUrl = IsMainnet() ? _selectedChain.MainnetRpcUrl : _selectedChain.TestnetRpcUrl;
        var queryResult = _bloctoWalletProvider.QueryForSmartContract<int>(
            abiUrl, 
            _envDL.value == 0 ? _selectedChain.MainnetContractAddress : _selectedChain.TestnetContractAddress,
            "value");
        _valueResultTxt.text = queryResult.ToString();
    }

    private void SetValue()
    {
        var address = _accountTxt.text;
        var abiUrl = new Uri($"{_selectedChain.TestnetExplorerApiUrl}/api?module=contract&action=getabi&address={_selectedChain.TestnetContractAddress}");
        var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
        
        var web3 = new Web3(IsMainnet() ? _selectedChain.MainnetRpcUrl : _selectedChain.TestnetRpcUrl);
        var contract = web3.Eth.GetContract(api.Result, IsMainnet() ? _selectedChain.MainnetContractAddress : _selectedChain.TestnetContractAddress);
        var setValue = contract.GetFunction("setValue");
        var data = setValue.GetData(new object[]{ Convert.ToUInt64(_setValueTxt.text) });
        $"Set Value: {_setValueTxt.text}, encode data: {data}".ToLog();
        _bloctoWalletProvider.SendTransaction(
            address, 
            _envDL.value == 0 ? _selectedChain.MainnetContractAddress : _selectedChain.TestnetContractAddress, 
            0, 
            data, 
            txId => {
                $"TxId: {txId}".ToLog();
                _setValueResultTxt.text = txId;
            });
    }
    
    private bool IsMainnet()
    {
        return _envDL.value == 0 ? true : false;
    }
    
    private void SetValueOpenExplorer()
    {
        var url = $"https://{_selectedChain.TestnetExplorerDomain}/tx/{_setValueResultTxt.text}";
        Application.OpenURL(url);
    }
    
    private void EthSignSample()
    {
        _signMessageTxt.text = _ethSignSample.EthSign;
        _signType = SignTypeEnum.Eth_Sign;
    }
    
    private void PersonalSignSample()
    {
        _signMessageTxt.text = _ethSignSample.PersonalSign;
        _signType = SignTypeEnum.Personal_Sign;
    }
    
    private void TypedDataV3()
    {
        _signMessageTxt.text = _ethSignSample.TypedDataV3;
        _signType = SignTypeEnum.SignTypedDataV3;
    }
    
    private void TypedDataV4()
    {
        _signMessageTxt.text = _ethSignSample.TypedDataV4;
        _signType = SignTypeEnum.SignTypedDataV4;
    }
    
    private void TypedData()
    {
        _signMessageTxt.text = _ethSignSample.TypedData;
        _signType = SignTypeEnum.SignTypedData;
    }
    
}