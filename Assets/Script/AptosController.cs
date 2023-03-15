using System;
using Blocto.Sdk.Aptos;
using Blocto.Sdk.Aptos.Model;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class AptosController : MonoBehaviour
{
    private BloctoWalletProvider _bloctoWalletProvider;
    
    private Button _connectWalletBtn;
    
    private Button _transferBtn;
    
    private InputField _walletTxt;
    
    private string _walletAddress;

    public void Awake()
    {
        "This is Aptos controller.".ToLog();
        var tmp = GameObject.Find("ConnectWalletBtn");
        _connectWalletBtn = tmp.GetComponent<Button>();
        _connectWalletBtn.onClick.AddListener(ConnectWallet);
        
        tmp = GameObject.Find("TransferBtn");
        _transferBtn = tmp.GetComponent<Button>();
        _transferBtn.onClick.AddListener(SendTransaction);
        
        
        tmp = GameObject.Find("WalletTxt");
        _walletTxt = tmp.GetComponent<InputField>();
        
        _bloctoWalletProvider = BloctoWalletProvider.CreateBloctoWalletProvider(
            gameObject: gameObject,
            env: EnvEnum.Devnet,
            bloctoAppIdentifier:Guid.Parse("4271a8b2-3198-4646-b6a2-fe825f982220")
        ); 
    }

    private void ConnectWallet()
    {
        _bloctoWalletProvider.RequestAccount(address => {
                                                 Debug.Log($"Address: {address}");
                                                 _walletAddress = address;
                                                 _walletTxt.text = address;
                                                 ;                                            }); 
    }
    
    private void SendTransaction()
    {
        var random = new Random();
        var tmp = random.Next(10, 20);
        var amount = tmp.ToString().PadRight(7, '0');
        $"Amount: {amount}".ToLog();
        
        var transaction = new EntryFunctionTransactionPayload
                          {
                              Address = "0x8f34b15e37d40490045770361dda638679c9b60029605829524b3c5d7093359b", 
                              Arguments = new string[] { "0x49e39c396ee0f1ace3990968cd9991b6bec0aa6f329afc9d1fa79757839a703f", amount },
                              TypeArguments = new string[] { BloctoWalletProvider.AptosCoinType },
                          };
        
        _bloctoWalletProvider.SendTransaction(transaction, tx => {
                                                               $"Tx: {tx}".ToLog();
                                                           });
    }
}
