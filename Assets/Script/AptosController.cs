using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using Blocto.Sdk.Aptos;
using Blocto.Sdk.Aptos.Model;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Chaos.NaCl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AptosController : MonoBehaviour
{
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private WebRequestUtility _webRequestUtility;
    
    private Button _connectWalletBtn;
    
    private Button _transferBtn;
    
    private Button _signBtn;
    
    private Button _verifySignatureBtn;
    
    private Button _getValueBtn;
    
    private Button _setValueBtn;
    
    private Button _menuBtn;
    
    private Button _transferOpenExplorerBtn;
    
    private Button _setValueOpenExplorerBtn;
    
    private InputField _walletTxt;
    
    private InputField _signMessageTxt;
    
    private InputField _receptionAddressTxt;
    
    private InputField _transferValueTxt;
    
    private InputField _transferResultTxt;
    
    private InputField _valueResultTxt;
    
    private InputField _setValueTxt;
    
    private InputField _setValueResultTxt;
    
    private string _walletAddress;
    
    private SignMessageResponse _signMessageResponse;

    public void Awake()
    {
        "This is Aptos controller.".ToLog();
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("TransferBtn");
        _transferBtn = tmp.GetComponent<Button>();
        _transferBtn.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignMessage);
        
        tmp = GameObject.Find("VerifyBtn");
        _verifySignatureBtn = tmp.GetComponent<Button>();
        _verifySignatureBtn.onClick.AddListener(VerifySignature);
        
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
        
        tmp = GameObject.Find("MenuBtn");
        _menuBtn = tmp.GetComponent<Button>();
        _menuBtn.onClick.AddListener(RetunMenu);
        
        tmp = GameObject.Find("WalletTxt");
        _walletTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signMessageTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _receptionAddressTxt = tmp.GetComponent<InputField>();
        
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
        
        _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
            gameObject: gameObject,
            env: EnvEnum.Devnet,
            bloctoAppIdentifier:Guid.Parse("38f6e448-308e-42b9-80aa-edb96a250113")
        ); 
        
        _webRequestUtility = gameObject.GetComponent<WebRequestUtility>();
    }

    private void ConnectWallet()
    {
        _bloctoWalletProvider.RequestAccount(address => {
                                                 $"Address is {address}".ToLog();
                                                 _walletAddress = address;
                                                 _walletTxt.text = address;
                                             }); 
    }
    
    private void SignMessage()
    {
        var signMessageRequest = new SignMessagePreRequest
                                 {
                                     Address = _walletAddress,
                                     Message = "I am Jamis",
                                     Nonce = "123456",
                                     IsIncludeAddress = false,
                                     IsIncludeApplication = false,
                                     IsIncludeChainId = false
                                 };
        _bloctoWalletProvider.SignMessage(signMessageRequest, signMessageResponse => {
                                                                  _signMessageTxt.text = "";
                                                                  foreach (var signature in signMessageResponse.Signatures)
                                                                  {
                                                                      _signMessageTxt.text += signature + "\r\n";
                                                                  }
                                                                  
                                                                  _signMessageResponse = signMessageResponse;
                                                                  Debug.Log($"SignMessage response: {JsonConvert.SerializeObject(_signMessageResponse)}");
                                                                  $"SignMessage response: {JsonConvert.SerializeObject(_signMessageResponse)}".ToLog();
                                                              });
    }
    
    private void VerifySignature()
    {
        var publicKeys = _bloctoWalletProvider.PublicKeys(_walletAddress);
        var bitmap = new BitArray(_signMessageResponse.Bitmap);
        var keyIndexs = bitmap.Cast<bool>().Select((item, index) => item ? index : -1).Where(p => p >= 0).ToList();
        var message = Encoding.UTF8.GetBytes(_signMessageResponse.FullMessage);
        var isVerify = true;
        for (var i = 0; i < _signMessageResponse.Signatures.Count; i++)
        {
            var key = publicKeys[keyIndexs[i]].HexToBytes();
            var signatureBytes = _signMessageResponse.Signatures[i].HexToBytes();
            isVerify = Ed25519.Verify(signatureBytes, message, key);
        }
          
        Debug.Log($"Signature verify is {isVerify}");
        $"Signature verify is: {isVerify}".ToLog();
    }
    
    private void GetValue()
    {
        var resourceAddress = "0xdaeab14fc79d6fdc60a1ea1d33e815c7d0e7736427224fd9a4c1415fa51d22b2";
        var resourceType = "0xdaeab14fc79d6fdc60a1ea1d33e815c7d0e7736427224fd9a4c1415fa51d22b2::value::Value";
        var url = $"https://fullnode.testnet.aptoslabs.com/v1/accounts/{resourceAddress}/resource/{resourceType}";
        var response = _webRequestUtility.GetResponse<JObject>(url, HttpMethod.Get, "application/json");
        $"Response: {response.GetValue("data:value")}".ToLog();
        
        var value = (long)response.SelectToken("data.value");
        $"Value: {value}".ToLog();
        _valueResultTxt.text = value.ToString();
    }
    
    private void SetValue()
    {
        var transaction = new EntryFunctionTransactionPayload
                          {
                              Address = _walletAddress, 
                              Function = "0xdaeab14fc79d6fdc60a1ea1d33e815c7d0e7736427224fd9a4c1415fa51d22b2::value::set_value",
                              Arguments = new object[] { _setValueTxt.text },
                              TypeArguments = new string[] {  },
                          }; 
        
        _bloctoWalletProvider.SendTransaction(transaction, tx => {
                                                               $"Tx: {tx}".ToLog();
                                                               _setValueResultTxt.text = tx;
                                                           });
    }
    
    private void SendTransaction()
    {
        var amount= Convert.ToDouble(_transferValueTxt) * 10000000;
        var transaction = new EntryFunctionTransactionPayload
                          {
                              Address = _walletAddress, 
                              Arguments = new object[] { _receptionAddressTxt.text, amount.ToString(CultureInfo.InvariantCulture) },
                              TypeArguments = new[] { BloctoWalletProvider.AptosCoinType },
                          };
        
        // var transaction = new ScriptTransactionPayload
        //                   {
        //                       Address = _walletAddress,
        //                       Arguments = new object[] { _receptionAddressTxt.text , "1"},
        //                       TypeArguments = new string[]{},
        //                       Code = new Code
        //                              {
        //                                  ByteCode = "0xa11ceb0b0500000005010002030205050706070d170824200000000100010003060c0503000d6170746f735f6163636f756e74087472616e736665720000000000000000000000000000000000000000000000000000000000000001000001050b000b010b02110002",
        //                                  Abi = new AbiPayload
        //                                        {
        //                                            Name = "main",
        //                                            Visibility = "public",
        //                                            IsEntry = true,
        //                                            GenericTypeParams = new string[]{},
        //                                            Params = new[]
        //                                                     {
        //                                                         "&signer", "address", "u64"
        //                                                     },
        //                                            Return = new string[] { }
        //                                        }
        //                              }
        //                   };
        //
        
        
        _bloctoWalletProvider.SendTransaction(transaction, tx => {
                                                               $"Tx: {tx}".ToLog();
                                                               _transferResultTxt.text = tx;
                                                           });
    }
    
    private void RetunMenu()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
    
    private void TransferOpenExplorer()
    {
        var url = $"https://explorer.aptoslabs.com/txn/{_transferResultTxt.text}";
        Application.OpenURL(url);
    }
    
    private void SetValueOpenExplorer()
    {
        var url = $"https://explorer.aptoslabs.com/txn/{_setValueResultTxt.text}";
        Application.OpenURL(url);
    }
}
