using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Utility;
using Flow.FCL;
using Flow.FCL.Config;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    static string _script = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";
    
    // static string _script = "transaction {prepare(signer: AuthAccount) { log(signer.address) }}";

    private string _url = "https://flow-wallet.blocto.app/authn?channel=back&authenticationId=h3KEsPgXh&fclVersion=1.1.0";
    
    private Button _authnBtn;
    
    private Button _sendTransaction;
    
    private Button _getAccountBtn;
    
    private Button _signBtn;
    
    private Button _queryBtn;
    
    private InputField _resultTxt;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private FlowClientLibrary _fcl;
    
    private IResolveUtils _resolveUtils;
    
    private ResolveUtility _resolveUtility;
    
    private void Awake()
    {
        Debug.Log("Start Debug.");
        var tmp = GameObject.Find("AuthnBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(ConnectionWallet);
        
        tmp = GameObject.Find("SendTransactionBtn");
        _sendTransaction = tmp.GetComponent<Button>();
        _sendTransaction.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignUserMessage);
        
        tmp = GameObject.Find("GetAccountBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(GetAccount);
        
        tmp = GameObject.Find("ResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("QueryBtn");
        _queryBtn = tmp.GetComponent<Button>();
        // _queryBtn.onClick.AddListener(ExecuteQuery);
        
        _queryBtn.onClick.AddListener(delegate { ExecuteQuery(); });
    }

    // Start is called before the first frame update
    void Start()
    {
        var config = new Config();
        config.Put("testnet", "https://rest-testnet.onflow.org/v1")
              .Put("discovery.wallet", "https://flow-wallet-testnet.blocto.app/api/flow/authn");
        _resolveUtils = gameObject.AddComponent<UtilFactory>().CreateResolveUtils();
        _resolveUtility = gameObject.AddComponent<UtilFactory>().CreateResolveUtility();
        _fcl = FlowClientLibrary.CreateClientLibrary(gameObject, config, "testnet", _resolveUtils);
    }
    
    
    private Task<string> GetBlock()
    {
        var result = _flowWebRequest.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        return Task.FromResult(result.Header.Id);
    }
    
    private void ConnectionWallet()
    {
        Debug.Log("In ConnectWallet.");
        $"Get address url : {_url}".ToLog();
        
        _fcl.Authenticate(() => {
                              var currentUser = _fcl.CurrentUser().Snapshot();
                              var data = currentUser.Services.First(p => p.Type == ServiceTypeEnum.AccountProof);
                              var signature = data.Data.Signatures.First().SignatureStr;
                              _resultTxt.text = $"Addr: {currentUser.Addr.Address}, Signature: {signature}";
                          });
    }
    
    private void SendTransaction()
    {
        var arguments = new List<BaseArgument>
                        {
                            new BaseArgument
                            {
                                Type = "UFix64",
                                Value = 7.5.ToString()
                            },
                            new BaseArgument
                            {
                                Type = "Address",
                                Value = "0xe2c2f0fd9fdec656"
                            }
                        };
        
        var tx = new FlowTransaction
                 {
                     Script = _script,
                     GasLimit = 1000,
                     Arguments = new List<ICadence>
                                 {
                                     new CadenceNumber(CadenceNumberType.UFix64, "7.5"),
                                     new CadenceAddress("0xe2c2f0fd9fdec656")
                                 },
                 };
        
        _fcl.Mutate(tx, () => {
                                       _resultTxt.text = _fcl.CurrentUser().GetLastTxId();
                                   });
    }
    
    public async Task ExecuteQuery()
    {
        var complexScript = @"
                            pub struct User {
                                pub var balance: UFix64
                                pub var address: Address
                                pub var name: String
                                

                                init(name: String, address: Address, balance: UFix64) {
                                    self.name = name
                                    self.address = address
                                    self.balance = balance
                                }
                            }

                            pub fun main(name: String): User {
                                return User(
                                    name: name,
                                    address: 0x1,
                                    balance: 10.0
                                )
                            }";
        var simpleScript = @"pub fun main(): Int { return 1 + 2 }";
        var flowScript = new FlowScript
                         {
                             Script = simpleScript,
                             // Arguments = new List<ICadence>
                             //             {
                             //                 new CadenceString("Jamis")
                             //             }
                         };
        
        var result = await _fcl.Query(flowScript);
        $"Result isSuccessed: {result.IsSuccessed}".ToLog();
        if(result.IsSuccessed)
        {
            //// complexScript result parser
            // var name = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceString>("name").Value;
            // var balance = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceNumber>("balance").Value;
            // var address = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceAddress>("address").Value;
            // _resultTxt.text = $"Name: {name}, Address: {address}";
            
            //// simplexScript result parser
            var value = result.Data.As<CadenceNumber>().Value;
            _resultTxt.text = $"Value: {value}";
        }
        else
        {
            _resultTxt.text = result.Message;
        }
    }
    
    private void SignUserMessage()
    {
        // _fcl.SignUserMessage();    
    }
    
    private async void GetAccount()
    {
        var account = await _fcl.GetAccount("f086a545ce3c552d");
        _resultTxt.text = $"Address: {account.Address.Address}, KeyId: {account.Keys.First().Index}, SeqNum: {account.Keys.First().SequenceNumber}";
    }
    
    public void DeeplinkHandler(string callbackStr)
    {
        Debug.Log($"Receive deeplink: {callbackStr}");
    }
}