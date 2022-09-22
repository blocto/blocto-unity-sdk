using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blocto.SDK.Flow;
using Blocto.Sdk.Flow.Utility;
using Flow.FCL;
using Flow.FCL.Config;
using Flow.FCL.Utility;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using Plugins.Flow.FCL.Models;
using UnityEngine;
using UnityEngine.UI;
using KeyGenerator = Blocto.Sdk.Core.Utility.KeyGenerator;
using Random = System.Random;

[SuppressMessage("ReSharper", "InterpolatedStringExpressionIsNotIFormattable")]
public class MainController : MonoBehaviour
{
    static string _script = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";
    
    static string _queryScript = @"
    import ValueDapp from 0x5a8143da8058740c

    pub fun main(): UFix64 {
        return ValueDapp.value
    }";
    
    static string _mutateScript = @"
    import ValueDapp from 0x5a8143da8058740c

    transaction(value: UFix64) {
        prepare(authorizer: AuthAccount) {
            ValueDapp.setValue(value)
        }
    }";
    
    private string _url = "https://flow-wallet.blocto.app/authn?channel=back&authenticationId=h3KEsPgXh&fclVersion=1.1.0";
    
    private Button _authnBtn;
    
    private Button _sendTransaction;
    
    private Button _getAccountBtn;
    
    private Button _signBtn;
    
    private Button _verifyMessageBtn;
    
    private Button _queryBtn;
    
    private Button _testBtn;
    
    private Button _transactionBtn;
    
    private InputField _accountTxt;
    
    private InputField _resultTxt;
    
    private InputField _signmessageTxt;
    
    private InputField _transactionToTxt;
    
    private InputField _transactionAmountTxt;
    
    private InputField _txResultTxt;
    
    private InputField _transactionValueTxt;
    
    private InputField _queryResultTxt;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private FlowClientLibrary _fcl;
    
    private IResolveUtility _resolveUtilities;
    
    private BloctoWalletProvider _walletProvider;
    
    private string _address;
    
    private string _signature;
    
    private int _keyId;
    
    private string _txId;
    
    private string _originMessage = "SignMessage Test";
    
    private string _signMessageStr;
    
    private FlowSignature _flowSignature;
    
    private void Awake()
    {
        Debug.Log("Start Debug.");
        var tmp = GameObject.Find("AuthnBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("SendTransactionBtn");
        _sendTransaction = tmp.GetComponent<Button>();
        _sendTransaction.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignUserMessage);
        
        tmp = GameObject.Find("VerifyBtn");
        _verifyMessageBtn = tmp.GetComponent<Button>();
        _verifyMessageBtn.onClick.AddListener(VerifyUserMessage);
        
        tmp = GameObject.Find("GetLastTxBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(GetTxr);
        
        tmp = GameObject.Find("TransactionBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(Transaction);
        
        tmp = GameObject.Find("TestBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(Test);
        
        tmp = GameObject.Find("ResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("AccountTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signmessageTxt = tmp.GetComponent<InputField>();
        _signmessageTxt.text = "SignMessage Test";
        
        tmp = GameObject.Find("TransactionAmountTxt");
        _transactionAmountTxt = tmp.GetComponent<InputField>();
        _transactionAmountTxt.text = ((new Random().Next(10, 40)) /10).ToString("N8");
        
        tmp = GameObject.Find("TransactionToTxt");
        _transactionToTxt = tmp.GetComponent<InputField>();
        _transactionToTxt.text = "0xe2c2f0fd9fdec656";
        
        tmp = GameObject.Find("TxResultTxt");
        _txResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("TransactionValueTxt");
        _transactionValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("QueryResultTxt");
        _queryResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("QueryBtn");
        _queryBtn = tmp.GetComponent<Button>();
        _queryBtn.onClick.AddListener(delegate { ExecuteQuery(); });
    }

    void Start()
    {
        var config = new Config();
        config.Put("discovery.wallet", "https://flow-wallet-testnet.blocto.app/api/flow/authn")
              .Put("accessNode.api", "https://rest-testnet.onflow.org/v1")
              .Put("fcl.limit", "1000")
              .Put("flow.network", "testnet");
        
        _walletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(initialFun: GetWallet => {
                                                                                          var walletProvider = GetWallet.Invoke(
                                                                                              gameObject, 
                                                                                              new FlowUnityWebRequest(gameObject, config.Get("accessNode.api")),
                                                                                              new ResolveUtility());
                                                                                          
                                                                                          return walletProvider;
                                                                                      }, 
                                                                          bloctoAppIdentifier:Guid.Parse("d0c4c565-db60-4848-99c8-2bdfc6bd3576"));
        
        _fcl = FlowClientLibrary.CreateClientLibrary(GetFCL => {
                                                                     var fcl = GetFCL.Invoke(gameObject, _walletProvider, new ResolveUtility());
                                                                     return fcl;
                                                                 }, config);
    }
    
    private void ConnectWallet()
    {
        var accountProofData = new AccountProofData
                               {
                                   AppId = "com.blocto.flow.unitydemo",
                                   Nonce = KeyGenerator.GetUniqueKey(32).StringToHex()
                               };
        
        _fcl.Authenticate(
            accountProofData:accountProofData,
            callback: ((currentUser,  accountProofData) => {
                           _accountTxt.text = currentUser.Addr.Address.AddHexPrefix();
                           if(accountProofData != null)
                           {
                               $"AppId: {accountProofData.AppId}, Nonce: {accountProofData.Nonce}".ToLog();
                               $"Address: {accountProofData.Signature.Addr}, KeyId: {accountProofData.Signature.KeyId}, Signature: {accountProofData.Signature.SignatureStr}".ToLog();
                           }
                           
                           var appUtil = new AppUtility(gameObject, new EncodeUtility());
                           var isVerify = appUtil.VerifyAccountProofSignature(
                               appIdentifier: accountProofData!.AppId,
                               accountProofData: accountProofData,
                               fclCryptoContract: "0x5b250a8a85b44a67");
                           Debug.Log($"User is verify: {isVerify}");
                       }));
    }
    
    private void SendTransaction()
    {
        var receiveAddress = _transactionToTxt.text;
        var transactionAmount = _transactionAmountTxt.text;
        
        var tx = new FlowTransaction
                 {
                     Script = _script,
                     GasLimit = 1000,
                     Arguments = new List<ICadence>
                                 {
                                     new CadenceNumber(CadenceNumberType.UFix64, $"{transactionAmount:N8}"),
                                     new CadenceAddress(receiveAddress.AddHexPrefix())
                                 },
                 };


        _fcl.Mutate(tx, txId => {
                            _txId = txId;
                            _resultTxt.text = $"https://testnet.flowscan.org/transaction/{_txId}";
                        });
    }
    
    private void Transaction()
    {
        var value = _transactionValueTxt.text;
        $"Value: {value.ToString()}".ToLog();
        var tx = new FlowTransaction
                 {
                     Script = MainController._mutateScript,
                     GasLimit = 1000,
                     Arguments = new List<ICadence>
                                 {
                                     new CadenceNumber(CadenceNumberType.UFix64, value.ToString())
                                 },
                 };
        
        _fcl.Mutate(tx, txId => { 
                            _txId = txId;
                            _resultTxt.text = $"https://testnet.flowscan.org/transaction/{_txId}";
                        });
    }
    
    public void GetTxr()
    {
        var result = _fcl.GetTransactionStatus(_txId);
        if(result.IsSuccessed)
        {
            switch (result.Data.Execution)
            {
                case TransactionExecution.Failure:
                    _txResultTxt.text = $"Execution: Failure \r\nErrorMessage: {result.Message}";
                    break;
                case TransactionExecution.Success:
                    _txResultTxt.text = $"BlockId: {result.Data.BlockId} \r\nExecution: {result.Data.Execution} \r\nStatus: {result.Data.Status}";
                    break;
                case TransactionExecution.Pending:
                    _txResultTxt.text = $"{result.Message}";
                    break;
            }
        }
    }
    
    public async Task ExecuteQuery()
    {
        var script = @" 
                    pub struct User {
                        pub var balance: UFix64
                        pub var address: Address
                        pub var name: String
                        pub var user: Item
                        
                        init(name: String, address: Address, balance: UFix64, item: Item) {
                            self.name = name
                            self.address = address
                            self.balance = balance
                            self.user = item
                        }
                    }

                    pub struct Item {
                        pub var name: String

                        init(name: String){
                            self.name = name
                        }
                    }

                    pub fun main(name: String): User {
                        return User(
                            name: name,
                            address: 0x1,
                            balance: 10.0,
                            item: Item(name: name)
                        )
                    }";
        
        var flowScript = new FlowScript
                         {
                             Script = MainController._queryScript,
                         };
        
        var result = await _fcl.QueryAsync(flowScript);
        if(result.IsSuccessed)
        {
            //// Composite object parser
            // var name = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceString>("name").Value;
            // var balance = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceNumber>("balance").Value;
            // var address = result.Data.As<CadenceComposite>().CompositeFieldAs<CadenceAddress>("address").Value;
            // _resultTxt.text = $"Name: {name}, Balance: {balance}, Address: {address}";
            
            var value = result.Data.As<CadenceNumber>().Value;
            _queryResultTxt.text = value;
        }
        else
        {
            _queryResultTxt.text = result.Message;
        }
    }
    
    public void VerifyUserMessage()
    {
        _signmessageTxt.text = "";
        var appUtil = new AppUtility(gameObject, new EncodeUtility());
        var result = appUtil.VerifyUserSignatures(_originMessage, _flowSignature, "0x5b250a8a85b44a67");
        _signmessageTxt.text += $"\r\nVerify result: {result}";
    }
    
    private void SignUserMessage()
    {
        _fcl.SignUserMessage(_signmessageTxt.text, result => 
                                                   {
                                                       if(result.IsSuccessed == false)
                                                       {
                                                           _signmessageTxt.text = $"Get signmessage failed, Reason: {result.Message}";
                                                           return;
                                                       }
                                                       
                                                       _flowSignature = result.Data;
                                                       _signmessageTxt.text = $"Message: {_originMessage} \r\nSignature: {Encoding.UTF8.GetString(_flowSignature.Signature)} \r\nKeyId: {_flowSignature.KeyId}";
                                                   });    
    }
    
    private async void GetAccount()
    {
        var account = await _fcl.FlowClient.GetAccountAtLatestBlockAsync("f086a545ce3c552d");
        _resultTxt.text = $"Address: {account.Address.Address}, KeyId: {account.Keys.First().Index}, SeqNum: {account.Keys.First().SequenceNumber}";
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            $"Get key [KeyCode.Escape]".ToLog();
            _walletProvider.CloseWebView();
        }
    }

    public void Test()
    {
        var result = _fcl.GetTransactionStatus("6ec486df690b1938fdfdc0a9ebf5c63fc39ad8202f48164252a57761fe11ee24");
        // _resolveUtils = new ResolveUtility(null);
        // var tx = new FlowTransaction
        //          {
        //              Script = _script,
        //              GasLimit = 9999,
        //              Arguments = new List<ICadence>
        //                          {
        //                              new CadenceNumber(CadenceNumberType.UFix64, "10.00000000"),
        //                              new CadenceAddress("0x068606b2acddc1ca")
        //                          },
        //              ProposalKey = new FlowProposalKey
        //                            {
        //                                Address = new FlowAddress("0xf086a545ce3c552d"),
        //                                KeyId = 0,
        //                                SequenceNumber = 458 
        //                            },
        //              ReferenceBlockId = "908476694b90bbcb184c4e98e76667aaf87ed477e0dfe31ecb62e3301872cd1f"
        //          }; 
        //
        // var preSignable = _resolveUtils.ResolvePreSignable(ref tx);
        // var preAuthzData = new PreAuthzData
        //                    {
        //                        Proposer = new AuthzInformation
        //                                   {
        //                                       Identity = new PreAuthzIdentity
        //                                                  {
        //                                                      KeyId = 0,
        //                                                      Address = "0xe2c2f0fd9fdec656"
        //                                                  }
        //                                   },
        //                        Payer = new List<AuthzInformation>
        //                                {
        //                                    new AuthzInformation
        //                                    {
        //                                        Identity = new PreAuthzIdentity
        //                                                   {
        //                                                       Address = "0xf086a545ce3c552d",
        //                                                       KeyId = 0 
        //                                                   }
        //                                    }
        //                                },
        //                        Authorization = new List<AuthzInformation>
        //                                        {
        //                                            new AuthzInformation
        //                                            {
        //                                                Identity = new PreAuthzIdentity
        //                                                           {
        //                                                               KeyId = 0,
        //                                                              Address = "0xe2c2f0fd9fdec656"
        //                                                           }
        //                                            }
        //                                        }
        //                    };
        //
        // var account = new FlowAccount
        //               {
        //                   Address = new FlowAddress("0xe2c2f0fd9fdec656"),
        //                   Keys = new List<FlowAccountKey>
        //                          {
        //                              new FlowAccountKey
        //                              {
        //                                  Index = 0,
        //                                  SequenceNumber = 0,
        //                                  Weight = 1000
        //                              }
        //                          }
        //               };
        //
        // var signable = _resolveUtils.ResolveSignable(ref tx, preAuthzData, account);
        // var payerSignable = _resolveUtils.ResolvePayerSignable(ref tx, signable);
    }
}