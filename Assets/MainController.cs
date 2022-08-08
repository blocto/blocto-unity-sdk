using System;
using System.Threading.Tasks;
using Blocto.Flow;
using Flow.FCL;
using Flow.FCL.WalletProvider;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.SDK.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    private string _url = "https://flow-wallet.blocto.app/authn?channel=back&authenticationId=h3KEsPgXh&fclVersion=1.1.0";
    
    private Button _authnBtn;
    
    private Button _signBtn;
    
    private InputField _resultTxt;
    
    private FlowUnityWebRequest _flowWebRequest;
    
    private FlowClientLibrary _fcl;
    
    private IWalletProvider _walletProvider;
    
    private void Awake()
    {
        Debug.Log("Start Debug.");
        var tmp = GameObject.Find("AuthnBtn");
        _authnBtn = tmp.GetComponent<Button>();
        _authnBtn.onClick.AddListener(ConnectionWallet);
        
        tmp = GameObject.Find("SignBtn");
        _signBtn = tmp.GetComponent<Button>();
        _signBtn.onClick.AddListener(SignUserMessage);
        
        tmp = GameObject.Find("ResultTxt");
        _resultTxt = tmp.GetComponent<InputField>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _fcl = gameObject.AddComponent<FlowClientLibrary>();
        _walletProvider = this.gameObject.AddComponent<BloctoWalletProvider>();
        _fcl.WalletProvider = _walletProvider;
        _walletProvider.Init(gameObject);
        
        _fcl.Config.Put("discovery.wallet", "https://flow-wallet-testnet.blocto.app/api/flow/authn");
    }
    
    
    private Task<string> GetBlock()
    {
        var result = _flowWebRequest.GetLatestBlockAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        return Task.FromResult(result.Header.Id);
    }
    
    private void ConnectionWallet()
    {
        Debug.Log("In ConnectWallet.");
        var appId = "978d58b5-9612-4b4e-a7de-69cdf8a7a1ba";
        var requestId = Guid.NewGuid();
        // var url = $"https://wallet-testnet.blocto.app/sdk?app_id={appId}&blockchain=ethereum&method=request_account&request_id={requestId}";

        $"Get address url : {_url}".ToLog();
        
        _fcl.Authenticate(() => {
                              var currentUser = _fcl.CurrentUser().Snapshot();
                              _resultTxt.text = currentUser.Addr.Address;
                          });
    }
    
    private void SignUserMessage()
    {
        // _fcl.SignUserMessage();   
    }
    
    public void DeeplinkHandler(string callbackStr)
    {
        Debug.Log($"Receive deeplink: {callbackStr}");
    }
}