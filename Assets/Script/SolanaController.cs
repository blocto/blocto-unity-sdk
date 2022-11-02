using System.Collections.Generic;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Solana;
using Script.Model;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using UnityEngine;
using UnityEngine.UI;

public class SolanaController : MonoBehaviour
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
    
    private WebRequestUtility _webRequestUtility;

    public void Awake()
    {
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("TestBtn");
        _testBtn = tmp.GetComponent<Button>();
        _testBtn.onClick.AddListener(TestEvent);
        
        tmp = GameObject.Find("AccountTxt");
        _accountTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
        
        _webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
        // _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
        //         gameObject: gameObject,
        //         env: "testnet",
        //         bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
        //     );
    }


    private void ConnectWallet()
    {
        // $"Connect wallet.".ToLog();
        // _bloctoWalletProvider.RequestAccount(address => {
        //                                         _walletAddreass = address;
        //                                         _accountTxt.text = address;
        //                                     });
    }
    
    private void SignMessage()
    {
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        // _bloctoWalletProvider.ForceUseWebView = value;
    }
    
    // public async Task<string> GetTestEventWrapper(SynchronizationContext _context)
    // {
    //     // var result = await GetRecentBlockIdAsync().ConfigureAwait(false);
    //     // $"Block hash: {result}".ToLog();
    //     
    //     TestEvent(_context).ConfigureAwait(false).GetAwaiter().GetResult();
    //     // _context.Post(_ =>{
    //     //                   textMesh.text = "GetHtmlAsyncWrapper";
    //     //               },null);
    //     $"Test processed.".ToLog();
    //     return "complete";
    // }
    
    private void TestEvent()
    {
        var solanaClient = ClientFactory.GetClient(Cluster.DevNet, _webRequestUtility);
        var response = solanaClient.GetLatestBlockHash();
        $"block hash: {response.Result.Value.Blockhash}".ToLog();
        
        var address = "CXxPxb5GAkqjVKxb3PkxFZmUus9YrTVuUoLWee4gm8ZR";
        var feePayer = new PublicKey("CXxPxb5GAkqjVKxb3PkxFZmUus9YrTVuUoLWee4gm8ZR");
        var blockHash = "5ado6ywC4oW6q5EMCSFKqr9CZ4EjrmAodxPUc1JHmUJt";
        
        var txInstruction = ValueProgram.CreateSetValaueInstruction(1111111, address);
        Transaction tx = new()
                         {
                             RecentBlockHash = blockHash,
                             FeePayer = feePayer,
                             Instructions = new List<TransactionInstruction>{ txInstruction }
                         };
        
        _bloctoWalletProvider.SignAndSendTransaction(address, tx);
    }
    
    // private async Task<string> GetRecentBlockIdAsync()
    // {
    //     // var block = await _bloctoWalletProvider.SolanaClient.GetLatestBlockHashAsync().ConfigureAwait(false);
    //     // return block.Result.Value.Blockhash;
    // }
}
