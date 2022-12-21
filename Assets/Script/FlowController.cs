using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Flow;
using Blocto.Sdk.Flow.Utility;
using Flow.FCL;
using Flow.FCL.Config;
using Flow.FCL.Models;
using Flow.FCL.Utility;
using Flow.Net.Sdk.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Models;
using UnityEngine;
using UnityEngine.UI;
using KeyGenerator = Blocto.Sdk.Core.Utility.KeyGenerator;
using Random = System.Random;

[SuppressMessage("ReSharper", "InterpolatedStringExpressionIsNotIFormattable")]
public class FlowController : MonoBehaviour
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
    
    private Button _authnBtn;
    
    private Button _sendTransaction;
    
    private Button _getAccountBtn;
    
    private Button _signBtn;
    
    private Button _verifyMessageBtn;
    
    private Button _queryBtn;
    
    private Button _testBtn;
    
    private Button _transactionBtn;
    
    private Button _openSetValueResultLinkBtn;
    
    private Button _openTransferResultLinkBtn;
    
    private InputField _accountTxt;
    
    private InputField _resultTxt;
    
    private InputField _signmessageTxt;
    
    private InputField _transactionToTxt;
    
    private InputField _transactionAmountTxt;
    
    private InputField _txResultTxt;
    
    private InputField _transactionValueTxt;
    
    private InputField _queryResultTxt;
    
    private InputField _setValueResultTxt;

    private Toggle _forceUseWebViewToggle;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private FlowClientLibrary _fcl;
    
    private IResolveUtility _resolveUtilities;
    
    private BloctoWalletProvider _walletProvider;
    
    private string _address;
    
    private string _signature;
    
    private int _keyId;
    
    private string _txId;
    
    private string _originMessage = "user input any message";
    
    private string _signMessageStr;
    
    private List<FlowSignature> _flowSignatures;
    
    private void Awake()
    {
        var tmp = GameObject.Find("ConnectWalletBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("TransferBtn");
        _sendTransaction = tmp.GetComponent<Button>();
        _sendTransaction.onClick.AddListener(SendTransaction);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignUserMessage);
        
        tmp = GameObject.Find("VerifyBtn");
        _verifyMessageBtn = tmp.GetComponent<Button>();
        _verifyMessageBtn.onClick.AddListener(VerifyUserMessage);
        
        tmp = GameObject.Find("SetValueBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(Transaction);
        
        tmp = GameObject.Find("TransferOpenExplorerBtn");
        _openTransferResultLinkBtn = tmp.GetComponent<Button>();
        _openTransferResultLinkBtn.onClick.AddListener(OpenTransferExplorer);
        
        tmp = GameObject.Find("SetValueOpenExplorerBtn");
        _openSetValueResultLinkBtn = tmp.GetComponent<Button>();
        _openSetValueResultLinkBtn.onClick.AddListener(OpenSetValueExplorer);
        
        tmp = GameObject.Find("WalletTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SignMessageTxt");
        _signmessageTxt = tmp.GetComponent<InputField>();
        _signmessageTxt.text = "user input any message";
        
        tmp = GameObject.Find("TransferValueTxt");
        _transactionAmountTxt = tmp.GetComponent<InputField>();
        _transactionAmountTxt.text = ((new Random().Next(10, 40)) /10).ToString("N8");
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _transactionToTxt = tmp.GetComponent<InputField>();
        _transactionToTxt.text = "0xe2c2f0fd9fdec656";
        
        tmp = GameObject.Find("SetValueTxt");
        _transactionValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("ValueResultTxt");
        _queryResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("TransferResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("SetValueResultTxt");
        _setValueResultTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("GetValueBtn");
        _queryBtn = tmp.GetComponent<Button>();
        _queryBtn.onClick.AddListener(delegate { ExecuteQuery(); });
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
    }

    void Start()
    {
        var config = new Config();
        config.Put("discovery.wallet", "https://flow-wallet-dev.blocto.app/api/flow/authn")
              .Put("accessNode.api", "https://rest-testnet.onflow.org/v1")
              .Put("flow.network", "testnet");
        
        _walletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(initialFun: GetWallet => {
                                                                                          var walletProvider = GetWallet.Invoke(
                                                                                              gameObject, 
                                                                                              new FlowUnityWebRequest(gameObject, "https://rest-testnet.onflow.org/v1"),
                                                                                              new ResolveUtility());
                                                                                          
                                                                                          return walletProvider;
                                                                                      }, 
                                                                          env: "testnet",
                                                                          bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")); 
        _fcl = FlowClientLibrary.CreateClientLibrary(GetFCL => {
                                                         var fcl = GetFCL.Invoke(gameObject, _walletProvider, new ResolveUtility());
                                                         return fcl;
                                                     }, config);
        _walletProvider.ForcedUseWebView = true;
        DontDestroyOnLoad (_walletProvider);
    }

    private void ConnectWallet()
    {
        $"Start ConnectWallet. {DateTime.Now:hh:mm:ss.fff}".ToLog();
        var accountProofData = new AccountProofData
                               {
                                   AppId = "com.blocto.flow.unitydemo",
                                   Nonce = KeyGenerator.GetUniqueKey(32).StringToHex()
                               };
        
        _fcl.Authenticate(
            accountProofData:accountProofData,
            callback: ((currentUser,  accountProofData) => {
                           _accountTxt.text = currentUser.Addr.Address.AddHexPrefix();
                           $"Start verify account proof".ToLog();
                           StartCoroutine(VerifyAccountProof(accountProofData));
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
                     SignerList =
                     {
                         
                     }
                 };


        _fcl.Mutate(tx, txId => {
                            _txId = txId;
                            _resultTxt.text = txId;
                        });
    }
    
    private void Transaction()
    {
        $"set value: {_transactionValueTxt.text}".ToLog();
        var value = Convert.ToDecimal(_transactionValueTxt.text);
        $"Value: {value:#0.00000000}".ToLog();
        
        var tx = new FlowTransaction
                 {
                     Script = FlowController._mutateScript,
                     GasLimit = 1000,
                     Arguments = new List<ICadence>
                                 {
                                     new CadenceNumber(CadenceNumberType.UFix64, $"{value:#0.00000000}")
                                 },
                 };
        
        _fcl.Mutate(tx, txId => { 
                            _txId = txId;
                            _setValueResultTxt.text = txId;
                        });
    }
    
    public void GetTxr()
    {
        $"Tx: {_txId}".ToLog();
        var result = _fcl.GetTransactionStatus(_txId);
        if(result.IsSuccessed)
        {
            switch (result.Data.Execution)
            {
                case TransactionExecution.Failure:
                    _txResultTxt.text = $"Execution: Failure \r\nErrorMessage: {result.Message}";
                    break;
                case TransactionExecution.Success:
                    _txResultTxt.text = $"Tx hash: {_txId} \r\nExecution: {result.Data.Execution} \r\nStatus: {result.Data.Status}";
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
                             Script = FlowController._queryScript,
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
        var result = appUtil.VerifyUserSignatures(_originMessage, _flowSignatures, "0x5b250a8a85b44a67");
        _signmessageTxt.text += $"\r\nVerify result: {result}";
    }
    
    private void SignUserMessage()
    {
        $"Sign user message".ToLog();
        _originMessage = _signmessageTxt.text;
        _fcl.SignUserMessage(_signmessageTxt.text, result => 
                                                   {
                                                       if(result.IsSuccessed == false)
                                                       {
                                                           _signmessageTxt.text = $"Get signmessage failed, Reason: {result.Message}";
                                                           return;
                                                       }
                                                       
                                                       _flowSignatures = result.Data;
                                                       _signmessageTxt.text = $"Message: {_originMessage} \r\nSignature: {Encoding.UTF8.GetString(_flowSignatures.First().Signature)} \r\nKeyId: {_flowSignatures.First().KeyId}";
                                                       foreach (var userSignature in result.Data)
                                                       {
                                                           Debug.Log($"Signature: {Encoding.UTF8.GetString(userSignature.Signature)} \r\nKeyId: {userSignature.KeyId}");
                                                       }
                                                   });    
    }
    
    private void OpenSetValueExplorer()
    {
        var url = $"https://testnet.flowscan.org/transaction/{_setValueResultTxt.text}";
        Application.OpenURL(url);
    }
    
    private void OpenTransferExplorer()
    {
        var url = $"https://testnet.flowscan.org/transaction/{_resultTxt.text}";
        Application.OpenURL(url);
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
            _walletProvider.CloseWebView();
        }
    }

    private IEnumerator VerifyAccountProof(AccountProofData accountProofData)
    {
        yield return new WaitForSeconds(0.5f);
        var appUtil = new AppUtility(gameObject, new EncodeUtility());
        var isVerify = appUtil.VerifyAccountProofSignature(
            appIdentifier: accountProofData!.AppId,
            accountProofData: accountProofData,
            fclCryptoContract: "0x5b250a8a85b44a67");
        Debug.Log($"User is verify: {isVerify}"); 
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        _walletProvider.ForcedUseWebView = value;
    }
}