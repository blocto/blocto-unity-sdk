using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.FCL;
using Flow.FCL.Config;
using Flow.FCL.Models;
using Flow.FCL.Models.Authz;
using Flow.FCL.Utility;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.SDK.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    static string _script = "import FungibleToken from 0x9a0766d93b6608b7\nimport FlowToken from 0x7e60df042a9c0868\n\ntransaction(amount: UFix64, to: Address) {\n\n    // The Vault resource that holds the tokens that are being transferred\n    let sentVault: @FungibleToken.Vault\n\n    prepare(signer: AuthAccount) {\n\n        // Get a reference to the signer's stored vault\n        let vaultRef = signer.borrow<&FlowToken.Vault>(from: /storage/flowTokenVault)\n            ?? panic(\"Could not borrow reference to the owner's Vault!\")\n\n        // Withdraw tokens from the signer's stored vault\n        self.sentVault <- vaultRef.withdraw(amount: amount)\n    }\n\n    execute {\n\n        // Get the recipient's public account object\n        let recipient = getAccount(to)\n\n        // Get a reference to the recipient's Receiver\n        let receiverRef = recipient.getCapability(/public/flowTokenReceiver)\n            .borrow<&{FungibleToken.Receiver}>()\n            ?? panic(\"Could not borrow receiver reference to the recipient's Vault\")\n\n        // Deposit the withdrawn tokens in the recipient's receiver\n        receiverRef.deposit(from: <-self.sentVault)\n    }\n}";
    
    // static string _script = "transaction {prepare(signer: AuthAccount) { log(signer.address) }}";

    private string _url = "https://flow-wallet.blocto.app/authn?channel=back&authenticationId=h3KEsPgXh&fclVersion=1.1.0";
    
    private Button _authnBtn;
    
    private Button _preAuthzBtn;
    
    private Button _getAccountBtn;
    
    private Button _signBtn;
    
    private InputField _resultTxt;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private FlowClientLibrary _fcl;
    
    private IResolveUtils _resolveUtils;
    
    private void Awake()
    {
        Debug.Log("Start Debug.");
        var tmp = GameObject.Find("AuthnBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(ConnectionWallet);
        
        tmp = GameObject.Find("PreAuthzBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(PreAuthz);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignUserMessage);
        
        tmp = GameObject.Find("GetAccountBtn");
        _getAccountBtn = tmp.GetComponent<Button>();
        _getAccountBtn.onClick.AddListener(GetAccount);
        
        tmp = GameObject.Find("ResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var config = new Config();
        config.Put("testnet", "https://rest-testnet.onflow.org/v1")
              .Put("discovery.wallet", "https://flow-wallet-testnet.blocto.app/api/flow/authn");
        _resolveUtils = gameObject.AddComponent<UtilFactory>().CreateResolveUtils();
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
    
    private void PreAuthz()
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
                                Value = "0x068606b2acddc1ca"
                            }
                        };
        
        var preSignable = _resolveUtils.ResolvePreSignable(arguments, MainController._script, 1000);
        // var proposer = new Account
        //                {
        //                    Addr = "f086a545ce3c552d",
        //                    KeyId = 1071,
        //                    TempId = $"f086a545ce3c552d-1071",
        //                    SequenceNum = 417 
        //                };
        // var payer = new List<Account>{proposer};
        // var authorization = new List<Account>()
        //                     {
        //                         new Account
        //                         {
        //                             Addr = "068606b2acddc1ca",
        //                             KeyId = 1,
        //                             TempId = "068606b2acddc1ca-1"
        //                         }
        //                     };
        // var signable = _resolveUtils.ResolveAuthorizerSignable(proposer, payer.First(), authorization);
        // var payerSignable = _resolveUtils.ResolvePayerSignable(payer.First(), authorization.First(), signable, "bc226d9495c99260f4daf592a79ec545fe9f80effd2371918c766fc7532096791c89a3f166d263485aec1fb0de530d6c938942acd3b7c8ce7dc35917580d5a46");
        $"PreSignabel f_type: {preSignable.F_Type}".ToLog();
        _fcl.PreAuthz(preSignable);
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