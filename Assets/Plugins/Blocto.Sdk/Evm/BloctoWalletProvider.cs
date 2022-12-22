using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm.Model;
using Blocto.Sdk.Evm.Model.Eth;
using Blocto.Sdk.Evm.Utility;
using Nethereum.Web3;
using UnityEngine;
using UnityEngine.Networking;

namespace Blocto.Sdk.Evm
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public ChainEnum Chain { get; set; }
        
        public EthereumClient EthereumClient { get; private set; }
        
        public string NodeUrl { get; set;}
        
        private static string env;
        
        private WebRequestUtility _webRequestUtility;
        
        private Action<string> _connectWalletCallback;
        
        private Action<string> _signMessageCallback;
        
        private Action<string> _sendTransactionCallback;
        
        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, string env, Guid bloctoAppIdentifier)
        {
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            bloctoWalletProvider._webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            bloctoWalletProvider._webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            try
            {
                bloctoWalletProvider.EthereumClient = new EthereumClient(bloctoWalletProvider._webRequestUtility);
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.isCancelRequest = false;
                bloctoWalletProvider.bloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env.ToLower();
            
                if(env.ToLower() == "testnet")
                {
                    bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                    bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.dev";
                    bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                    bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                } 
            
                if(Application.platform == RuntimePlatform.Android)
                {
                    bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
                }
            
                bloctoWalletProvider.isInstalledApp = bloctoWalletProvider.IsInstalledApp(BloctoWalletProvider.env);
                bloctoWalletProvider.ForceUseWebView = true;
            }
            catch (Exception e)
            {
                e.Message.ToLog();
                throw;
            }
            
            return bloctoWalletProvider;
        }
        
        public void RequestAccount(Action<string> callBack)
        {
            var url = default(string);
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "CONNECTWALLET");
            _connectWalletCallback = callBack;
            
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                     .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                     .Append($"method=request_account" + "&")
                     .Append($"request_id={requestId}");
                url = appSb.ToString();
                
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                 .Append($"method=request_account" + "&")
                 .Append($"request_id={requestId}");
            url = webSb.ToString();
            
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url));
        }
        
        public void SignMessage(string originMessage, SignTypeEnum signType, string address, Action<string> callback)
        {
            if(signType == SignTypeEnum.Eth_Sign)
            {
                originMessage = originMessage.ToHexUTF8();
            }
            
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SIGNMESSAGE");
            _signMessageCallback = callback;
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={bloctoAppIdentifier}" + "&")
                     .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                     .Append("method=sign_message" + "&")
                     .Append($"from={address}" + "&")
                     .Append($"type={signType.GetEnumDescription()}" + "&")
                     .Append($"message={Uri.EscapeUriString(originMessage)}" + "&")
                     .Append($"request_id={requestId}");
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb.ToString()));
                return;
            }
            
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={bloctoAppIdentifier}" + "&")
                 .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                 .Append("method=sign_message" + "&")
                 .Append($"from={address}" + "&")
                 .Append($"type={signType.GetEnumDescription()}" + "&")
                 .Append($"message={Uri.EscapeUriString(originMessage)}" + "&")
                 .Append($"request_id={requestId}");
            
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
        }
        
        public void SendTransaction(string fromAddress, string toAddress, decimal value, string data, Action<string> callback)
        {
            var transactionValue = EthConvert.ToWei(value);
            var valueHex = transactionValue.ToString("X");
            $"Transaction Value: {transactionValue}, to string: {transactionValue.ToString()}, Value hex: {valueHex}".ToLog();
            
            
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SENDTRANSACTION");
            _sendTransactionCallback = callback;
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={bloctoAppIdentifier}" + "&")
                     .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                     .Append("method=send_transaction" + "&")
                     .Append($"from={fromAddress}" + "&")
                     .Append($"to={toAddress}" + "&")
                     .Append($"value=0x{valueHex}" + "&")
                     .Append($"data={data}" + "&")
                     .Append($"request_id={requestId}");
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb.ToString()));
                return;
            }
            
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={bloctoAppIdentifier}" + "&")
                 .Append($"blockchain={Chain.ToString().ToLower()}" + "&")
                 .Append("method=send_transaction" + "&")
                 .Append($"from={fromAddress}" + "&")
                 .Append($"to={toAddress}" + "&")
                 .Append($"value=0x{valueHex}" + "&")
                 .Append($"data={data}" + "&")
                 .Append($"request_id={requestId}");
            
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb.ToString()));
        }
        
        public TResult QueryForSmartContract<TResult>(Uri abiUrl, string contractAddress, string queryMethod)
        {
            var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
            var web3 = new Web3(NodeUrl);
            var contract = web3.Eth.GetContract(api.Result, contractAddress);
            
            var funData = contract.GetFunction(queryMethod).CallAsync<TResult>().ConfigureAwait(false).GetAwaiter().GetResult();
            $"Contract value: {funData}".ToLog();
            
            return funData;
        }
        
        public void SetContractValue(Uri abiUrl,  string contractAddress, string method)
        {
            var api = _webRequestUtility.GetResponse<AbiResult>(abiUrl.ToString(), HttpMethod.Get, "application/json");
            var web3 = new Web3(NodeUrl);
            var contract = web3.Eth.GetContract(api.Result, contractAddress);
            var serValue = contract.GetFunction("setValue");
        }
        
        /// <summary>
        /// Universal link handler on receive universal link
        /// </summary>
        /// <param name="link">universal link data from iOS</param>
        public void UniversalLinkCallbackHandler(string link)
        {
            $"Universal Link: {link}, in Handler".ToLog();
            var decodeLink = UnityWebRequest.UnEscapeURL(link);
            var item = decodeLink.RequestId();
            var result = default(string);
            if(requestIdActionMapper.ContainsKey(item.RequestId))
            {
                var action = requestIdActionMapper[item.RequestId];
                switch (action)
                {
                    case "CONNECTWALLET":
                        result = UniversalLinkHandler(item.RemainContent, "address=");
                        _connectWalletCallback.Invoke(result);
                        break;
                    
                    case "SIGNMESSAGE":
                        var signature = UniversalLinkHandler(item.RemainContent, "signature=");
                        _signMessageCallback.Invoke(signature);
                        break;
                    case "SENDTRANSACTION":
                        var sendTransaction = UniversalLinkHandler(item.RemainContent, "tx_hash");
                        _sendTransactionCallback.Invoke(sendTransaction);
                        break;
                }
            }
        }
        
        private string UniversalLinkHandler(string link, string keyword)
        {
            var result = default(string);
            var data = (MatchContents: new List<string>(), RemainContent: link);
            data = CheckContent(data.RemainContent, keyword);
            result = data.MatchContents.FirstOrDefault().AddressParser().Value;
            return result;
        }
    }
}