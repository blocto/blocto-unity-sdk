using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Flow.Model;
using Blocto.Sdk.Solana.Model;
using Newtonsoft.Json;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using ByteExtension = Blocto.Sdk.Core.Extension.ByteExtension;
using Transaction = Solnet.Rpc.Models.Transaction;

namespace Blocto.Sdk.Solana
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public IRpcClient SolanaClient { get; set; }
        
        private static EnvEnum env;
        
        private static string walletProgramId = "JBn9VwAiqpizWieotzn6FjEXrBu4fDe2XFjiFqZwp8Am";
        
        private static readonly string chainName = "solana";
        
        private WebRequestUtility _webRequestUtility;
        
        private Action<string> _connectWalletCallback;
        
        private Action<string> _signMessageCallback;
        
        private Action<string> _sendTransactionCallback;
        
        private Dictionary<string, Dictionary<string, string>> _appendTxdict;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="env">Env</param>
        /// <param name="bloctoAppIdentifier">Blocto sdk appId</param>
        /// <returns>BloctoWalletProvider</returns>
        public static BloctoWalletProvider CreateBloctoWalletProvider(GameObject gameObject, EnvEnum env, Guid bloctoAppIdentifier)
        {
            var bloctoWalletProvider = gameObject.AddComponent<BloctoWalletProvider>();
            var webRequestUtility = gameObject.AddComponent<WebRequestUtility>();
            webRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
            try
            {
                bloctoWalletProvider._webRequestUtility = webRequestUtility;
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.isCancelRequest = false;
                bloctoWalletProvider.bloctoAppIdentifier = bloctoAppIdentifier;
                BloctoWalletProvider.env = env;
            
                switch (env)
                {
                    case EnvEnum.Devnet:
                        bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                        bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.dev";
                        bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                        bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                        BloctoWalletProvider.walletProgramId = "Ckv4czD7qPmQvy2duKEa45WRp3ybD2XuaJzQAWrhAour";
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.DevNet, webRequestUtility);
                        break;
                    case EnvEnum.Mainnet:
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.MainNet, webRequestUtility);
                        break;
                    case EnvEnum.Testnet:
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.TestNet, webRequestUtility);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(env), env, null);
                }

                if(Application.platform == RuntimePlatform.Android)
                {
                    bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
                }
            
                bloctoWalletProvider.isInstalledApp = bloctoWalletProvider.IsInstalledApp(BloctoWalletProvider.env.ToString().ToLower());
                bloctoWalletProvider._appendTxdict = new Dictionary<string, Dictionary<string, string>>();
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
                $"Use app sdk.".ToLog();
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                     .Append($"blockchain={BloctoWalletProvider.chainName}" + "&")
                     .Append($"method={ActionNameEnum.Request_Account.ToString().ToLower()}" + "&")
                     .Append($"request_id={requestId}");
                url = appSb.ToString();
                
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"blockchain={BloctoWalletProvider.chainName}" + "&")
                 .Append($"method={ActionNameEnum.Request_Account.ToString().ToLower()}" + "&")
                 .Append($"request_id={requestId}");
            url = webSb.ToString();
            
            $"Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url, ForceUseWebView));
        }
        
        
        public void SignAndSendTransaction(string fromAddress, Transaction transaction, Action<string> callBack)
        {
            var url = default(string);
            var requestId = Guid.NewGuid();
            requestIdActionMapper.Add(requestId.ToString(), "SIGNANDSENETRANSACTION");
            _sendTransactionCallback = callBack;
            
            var isInvokeWrapped = transaction.Instructions.Any(p => Base58Encoding.Encode(p.ProgramId) == BloctoWalletProvider.walletProgramId);
            var tmp = new TransactionBuilder()
                     .SetRecentBlockHash(transaction.RecentBlockHash)
                     .SetFeePayer(transaction.FeePayer);

            foreach (var instruction in transaction.Instructions)
            {
                tmp.AddInstruction(instruction);
            }
            
            var pubKeySignaturePairs = new Dictionary<string, string>();
            
            if(transaction.Signatures != null)
            {
                pubKeySignaturePairs = transaction.Signatures.Where(p => p.Signature != null).Distinct(p => p.PublicKey).ToDictionary(pubKeyPair => pubKeyPair.PublicKey.Key, pubKeyPair => ByteExtension.ToHex(pubKeyPair.Signature));
            }

            var tx = tmp.BuildExecludeSign();
            var right = Uri.EscapeDataString("[");
            var left = Uri.EscapeDataString("]");
            var message = ByteExtension.ToHex(tx);
            
            $"Message: {message}".ToLog();
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = new StringBuilder(appSdkDomain);
                appSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                     .Append($"request_id={requestId}" + "&")
                     .Append($"method={ActionNameEnum.Sign_And_Send_Transaction.ToString().ToLower()}" + "&")
                     .Append($"blockchain={chainName}" + "&")
                     .Append($"from={fromAddress}" + "&")
                     .Append($"message={message}" + "&")
                     .Append($"is_invoke_wrapped={isInvokeWrapped.ToString().ToLower()}" + "&");
                
                foreach (var queryStr in pubKeySignaturePairs.Select(pair => $"public_key_signature_pairs{right}{pair.Key}{left}={pair.Value}"))
                {
                    appSb.Append($"{queryStr}" + "&");
                }

                foreach (var queryStr in _appendTxdict.SelectMany(appendTx => appendTx.Value.Select(item => $"append_tx{right}{item.Key}{left}={item.Value}")))
                {
                    appSb.Append($"{queryStr}" + "&");
                }
                
                url = appSb.ToString();
                url = url.Remove(url.Length-1, 1);
                _appendTxdict.Clear();
                $"Url: {url}".ToLog();
                StartCoroutine(OpenUrl(url));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = new StringBuilder(webSdkDomain);
            webSb.Append($"app_id={Uri.EscapeUriString(bloctoAppIdentifier.ToString())}" + "&")
                 .Append($"request_id={requestId}" + "&")
                 .Append($"method={ActionNameEnum.Sign_And_Send_Transaction.ToString().ToLower()}" + "&")
                 .Append($"blockchain={chainName}" + "&")
                 .Append($"from={fromAddress}" + "&")
                 .Append($"message={message}" + "&")
                 .Append($"is_invoke_wrapped={isInvokeWrapped.ToString().ToLower()}" + "&");
                
            
            foreach (var queryStr in pubKeySignaturePairs.Select(pair => $"public_key_signature_pairs{right}{pair.Key}{left}={pair.Value}"))
            {
                webSb.Append($"{queryStr}" + "&");
            }

            foreach (var queryStr in _appendTxdict.SelectMany(appendTx => appendTx.Value.Select(item => $"append_tx{right}{item.Key}{left}={item.Value}")))
            {
                webSb.Append($"{queryStr}" + "&");
            }
                
            url = webSb.ToString();
            url = url.Remove(url.Length-1, 1);
            _appendTxdict.Clear();
            $"ForcedUseWebView: {ForceUseWebView}, Url: {url}".ToLog();
            StartCoroutine(OpenUrl(url, ForceUseWebView));
        }
        
        public Transaction ConvertToProgramWalletTransaction(string address, Transaction transaction)
        {
            var tmp = new TransactionBuilder()
                     .SetRecentBlockHash(transaction.RecentBlockHash)
                     .SetFeePayer(transaction.FeePayer);

            foreach (var instruction in transaction.Instructions)
            {
                tmp.AddInstruction(instruction);
            }
            
            var tx = tmp.BuildExecludeSign();
            var request = new CreateRawTxRequest
                          {
                              Address = address,
                              RawTx = tx.Select(b => (sbyte)b).ToArray().ToHex()
                          };
            
            var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            var uploadHandler = new UploadHandlerRaw(requestBytes); 
            var webRequest = _webRequestUtility.CreateUnityWebRequest($"{backedApiDomain}/solana/createRawTransaction" , "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<CreateRawTxResponse>(webRequest);
            var messageBytes = response.RawTx.HexToByteArray();
            var message = Message.Deserialize(messageBytes);
            
            var programTransaction = new Transaction
                                     {
                                         RecentBlockHash = message.RecentBlockhash
                                     };
            
            if(Convert.ToInt32(message.Header.RequiredSignatures) > 0)
            {
                programTransaction.FeePayer = message.AccountKeys[0];
            }

            var accountKeys = new List<string>();
            foreach (var instruction in message.Instructions)
            {
                var keys = instruction.KeyIndices.Select(indices => {
                                                             var publicKey = message.AccountKeys[Convert.ToInt32(indices)];
                                                             var isSigner = Convert.ToInt32(indices) < Convert.ToInt32(message.Header.RequiredSignatures);
                                                             var accountMeta = new AccountMeta(publicKey, message.IsAccountWritable(Convert.ToInt32(indices)), isSigner);
                                                             return accountMeta;
                                                         }).ToList();
                accountKeys.AddRange(keys.Select(p => p.PublicKey));
                var transactionInstruction = new TransactionInstruction
                                             {
                                                 ProgramId = message.AccountKeys[instruction.ProgramIdIndex],
                                                 Keys = keys,
                                                 Data = instruction.Data
                                             };
                
                programTransaction.Add(transactionInstruction);
            }
            
            var key = programTransaction.CompileMessage();
            _appendTxdict.Add(ByteExtension.ToHex(key), response.ExtraData.AppendData);
            var tmpMessage = Message.Deserialize(key);
            
            var signedKeys = tmpMessage.AccountKeys.Take(tmpMessage.Header.RequiredSignatures);
            var signatures = signedKeys.Select(signedKey => new SignaturePubKeyPair { PublicKey = signedKey }).ToList();
            programTransaction.Signatures = signatures;

            return programTransaction;
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
                    
                    case "SIGNANDSENETRANSACTION":
                        $"In Sign and SendTransaction, remain content: {item.RemainContent}".ToLog();
                        var signature = UniversalLinkHandler(item.RemainContent, "tx_hash=");
                        _sendTransactionCallback.Invoke(signature);
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
        
        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            var str = sb.ToString();
            str.ToLog();
        }
    }
}