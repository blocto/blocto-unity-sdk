using System;
using System.Collections.Generic;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Solana;
using Script.Model;
using Solnet.Programs;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class SolanaController : MonoBehaviour
{
    
    private List<string> _words = new List<string>
                                  {
                                      "route", "clerk", "disease", "box", "emerge", "airport", "loud", "waste", "attitude",
                                      "film", "army", "tray", "forward", "deal", "onion", "eight", "catalog", "surface",
                                      "unit", "card", "window", "walnut", "wealth", "medal", "surprise", "learn", "search",
                                      "nurse", "woman"
                                  };
    
    private string _mnemonicWords;
    
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private Button _connectWalletBtn;
    
    private Button _transferBtn;
    
    private Button _getValueBtn;
    
    private Button _setValueBtn;
    
    private Button _partialSignBtn;
    
    private InputField _walletTxt;
    
    private InputField _receptionAddressTxt;
    
    private InputField _transferValueTxt;
    
    private InputField _transferResultTxt;
    
    private InputField _getValueResultTxt;
    
    private InputField _transactonResultTxt;
    
    private InputField _setValueTxt;
    
    private Toggle _forceUseWebViewToggle;
    
    private const string _originMessage = "blocto demo app.";
    
    private string _walletAddreass;
    
    private WebRequestUtility _webRequestUtility;

    public void Awake()
    {
        _mnemonicWords = string.Join(" ", _words.GetRandom(24));
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("WalletTxt");
        _walletTxt = tmp.GetComponent<InputField>();
        _walletTxt.readOnly = true;
        
        tmp = GameObject.Find("TransferBtn");
        _transferBtn = tmp.GetComponent<Button>();
        _transferBtn.onClick.AddListener(Transfer);
        
        tmp = GameObject.Find("ReceptionAddressTxt");
        _receptionAddressTxt = tmp.GetComponent<InputField>();
        _receptionAddressTxt.readOnly = true;
        _receptionAddressTxt.text = "7YJX4s9uxnaXP6DmmRhAPWyptNvq9HCAtfAVUPMEq7GU";
        
        tmp = GameObject.Find("TransferValueTxt");
        _transferValueTxt = tmp.GetComponent<InputField>();
        var value = (Decimal)(new Random().Next(1, 9));
        var randomSol = (value / 100).ToString("N2");
        _transferValueTxt.text = randomSol;
        
        tmp = GameObject.Find("TransferOpenExpolrerBtn");
        _partialSignBtn = tmp.GetComponent<Button>();
        _partialSignBtn.onClick.AddListener(TransferOpenExpolorer);
        
        tmp = GameObject.Find("TransferResultTxt");
        _transferResultTxt = tmp.GetComponent<InputField>();
        _transferResultTxt.readOnly = true;
        
        tmp = GameObject.Find("SetValueBtn");
        _setValueBtn = tmp.GetComponent<Button>();
        _setValueBtn.onClick.AddListener(SetValue);
        
        tmp = GameObject.Find("SetValueTxt");
        _setValueTxt = tmp.GetComponent<InputField>();
        
        tmp = GameObject.Find("GetValueBtn");
        _getValueBtn = tmp.GetComponent<Button>();
        _getValueBtn.onClick.AddListener(GetValue);
        
        tmp = GameObject.Find("ValueResultTxt");
        _getValueResultTxt = tmp.GetComponent<InputField>();
        _getValueResultTxt.readOnly = true;
        
        tmp = GameObject.Find("PartialSignBtn");
        _partialSignBtn = tmp.GetComponent<Button>();
        _partialSignBtn.onClick.AddListener(CreateAccountAndPartialSign);
        
        tmp = GameObject.Find("TransactionResultTxt");
        _transactonResultTxt = tmp.GetComponent<InputField>();
        _transactonResultTxt.readOnly = true;
        
        tmp = GameObject.Find("OpenExpolrerBtn");
        _partialSignBtn = tmp.GetComponent<Button>();
        _partialSignBtn.onClick.AddListener(OpenExpolorer);
        
        tmp = GameObject.Find("WebSdkToggle");
        _forceUseWebViewToggle = tmp.GetComponent<Toggle>();
        _forceUseWebViewToggle.onValueChanged.AddListener(ForceUseWebView);
        
        _webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
        _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
                gameObject: gameObject,
                env: "testnet",
                bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
            );
    }

    private void ConnectWallet()
    {
        _bloctoWalletProvider.RequestAccount(address => {
                                                _walletAddreass = address;
                                                _walletTxt.text = address;
;                                            });
    }
    
    private void SetValue()
    {
        var solanaClient = ClientFactory.GetClient(Cluster.DevNet, _webRequestUtility);
        var response = solanaClient.GetLatestBlockHash();
        var minimumBalance = solanaClient.GetMinimumBalanceForRentExemption(10);
        
        
        var feePayer = new PublicKey(_walletAddreass);
        var value = Convert.ToInt32(_setValueTxt.text);
        $"Wallet address: {_walletAddreass}, block hash: {response.Result.Value.Blockhash}, set value: {value}".ToLog();
        var tmp = ValueProgram.CreateSetValaueInstruction(value, _walletAddreass);
        Transaction tx = new()
                         {
                             RecentBlockHash = response.Result.Value.Blockhash,
                             FeePayer = feePayer,
                             Instructions = new List<TransactionInstruction>{ tmp }
                         };
        
        _bloctoWalletProvider.SignAndSendTransaction(_walletAddreass, tx, txhash => {
                                                                         $"Tx hash: {txhash}".ToLog();
                                                                         _transactonResultTxt.text = txhash;
                                                                     }); 
    }
    
    private void Transfer()
    {
        "In SendTransaction".ToLog();
        var solanaClient = ClientFactory.GetClient(Cluster.DevNet, _webRequestUtility);
        var response = solanaClient.GetLatestBlockHash();
        var minimumBalance = solanaClient.GetMinimumBalanceForRentExemption(10);
        
        var feePayer = new PublicKey(_walletAddreass);
        var realSol = Convert.ToUInt64(Convert.ToDecimal(_transferValueTxt.text) * 1000000000);
        $"From {_walletAddreass} transfer to {_receptionAddressTxt.text}, token value: {_transferValueTxt.text}, SOL: {realSol}".ToLog();
        var instructions = new List<TransactionInstruction>
                           {
                               SystemProgram.Transfer(new PublicKey(_walletAddreass), new PublicKey(_receptionAddressTxt.text), realSol),
                               // MemoProgram.NewMemo(new PublicKey(_walletAddreass), "Hello from jamis test.")
                           };
        var tx = new Transaction
                 {
                     RecentBlockHash = response.Result.Value.Blockhash,
                     FeePayer = feePayer,
                     Instructions = instructions
                 };
        
        
        _bloctoWalletProvider.SignAndSendTransaction(_walletAddreass, tx, txHash => {
                                                                              _transferResultTxt.text = txHash;
                                                                          });
    }

    private void GetValue()
    {
        var account = _bloctoWalletProvider.SolanaClient.GetAccountInfo(ValueProgram.ACCOUNT_PUBLIC_KEY_DEVNET);
        var value = Parser<UInt32>(Convert.FromBase64String(account.Result.Value.Data[0]));
        _getValueResultTxt.text = value.ToString();
    }
    
    private void OpenExpolorer()
    {
        var url = $"https://explorer.solana.com/tx/{_transactonResultTxt.text}?cluster=devnet";
        Application.OpenURL(url);
    }
    
    private void TransferOpenExpolorer()
    {
        var url = $"https://explorer.solana.com/tx/{_transferResultTxt.text}?cluster=devnet";
        Application.OpenURL(url);
    }
    
    private TType Parser<TType>(ReadOnlySpan<Byte> data)
    {
        var isInit = data.GetU8(0);
        var value = data.GetU32(1);
        return (TType)Convert.ChangeType(value, typeof(TType));
    }
    
    private void ForceUseWebView(bool value)
    {
        $"Toggle value: {value}".ToLog();
        _bloctoWalletProvider.ForceUseWebView = value;
    }
    
    private void CreateAccountAndPartialSign()
    {
        var solanaClient = ClientFactory.GetClient(Cluster.DevNet, _webRequestUtility);
        var response = solanaClient.GetLatestBlockHash();
        var minimumBalance = solanaClient.GetMinimumBalanceForRentExemption(10);
        
        $"block hash: {response.Result.Value.Blockhash}".ToLog();
        
        var feePayer = new PublicKey(_walletAddreass);
        var value = Convert.ToInt32(_setValueTxt.text);
        var txInstruction = ValueProgram.CreateSetValaueInstruction(value, _walletAddreass);
        
        var wallet = new Wallet(_mnemonicWords);
        var index = (new Random().Next(1, 1000));
        var newAccount = wallet.GetAccount(index);
        $"New account: {newAccount.PublicKey}".ToLog();
        var createAccountInstruction = SystemProgram.CreateAccount(
            new PublicKey(_walletAddreass), 
            newAccount,
            minimumBalance.Result,
            10,
            ValueProgram.ProgramId());
        Transaction tx = new()
                         {
                             RecentBlockHash = response.Result.Value.Blockhash,
                             FeePayer = feePayer,
                             Instructions = new List<TransactionInstruction>{ txInstruction, createAccountInstruction }
                         };
        
        var newTx = _bloctoWalletProvider.ConvertToProgramWalletTransaction(_walletAddreass, tx);
        newTx.PartialSign(new List<Account>{ newAccount });
        _bloctoWalletProvider.SignAndSendTransaction(_walletAddreass, newTx, txhash => {
                                                                      $"Tx hash: {txhash}".ToLog();
                                                                      _transactonResultTxt.text = txhash;
                                                                  }, new List<Account>{ newAccount });
    }
    
    // private async Task<string> GetRecentBlockIdAsync()
    // {
    //     // var block = await _bloctoWalletProvider.SolanaClient.GetLatestBlockHashAsync().ConfigureAwait(false);
    //     // return block.Result.Value.Blockhash;
    // }
}
