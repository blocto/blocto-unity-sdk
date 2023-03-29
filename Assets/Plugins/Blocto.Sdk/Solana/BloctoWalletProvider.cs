using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Solana.Model;
using Newtonsoft.Json;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using Transaction = Solnet.Rpc.Models.Transaction;

namespace Blocto.Sdk.Solana
{
    public class BloctoWalletProvider : BaseWalletProvider
    {
        public IRpcClient SolanaClient { get; set; }
        
        private string walletProgramId = "JBn9VwAiqpizWieotzn6FjEXrBu4fDe2XFjiFqZwp8Am";

        private const string chainName = "solana";
        
        private EnvEnum _env;

        private WebRequestUtility _webRequestUtility;
        
        private Action<string> _signMessageCallback;
       
        private Dictionary<string, Dictionary<string, string>> _appendTxDict;

        /// <summary>
        /// Create blocto wallet provider instance
        /// </summary>
        /// <param name="gameObject">Main scene gameobject</param>
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
                bloctoWalletProvider._env = env;
            
                switch (env)
                {
                    case EnvEnum.Devnet:
                        bloctoWalletProvider.backedApiDomain = bloctoWalletProvider.backedApiDomain.Replace("api", "api-dev");
                        bloctoWalletProvider.androidPackageName = $"{bloctoWalletProvider.androidPackageName}.dev";
                        bloctoWalletProvider.appSdkDomain = bloctoWalletProvider.appSdkDomain.Replace("blocto.app", "dev.blocto.app");
                        bloctoWalletProvider.webSdkDomain = bloctoWalletProvider.webSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                        bloctoWalletProvider.walletProgramId = "Ckv4czD7qPmQvy2duKEa45WRp3ybD2XuaJzQAWrhAour";
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.DevNet, webRequestUtility);
                        bloctoWalletProvider.webSdkDomainV2 = bloctoWalletProvider.webSdkDomainV2.Replace("wallet-v2.blocto.app", "wallet-v2-dev.blocto.app");
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
            
                bloctoWalletProvider.isInstalledApp = bloctoWalletProvider.IsInstalledApp(bloctoWalletProvider._env);
                bloctoWalletProvider._appendTxDict = new Dictionary<string, Dictionary<string, string>>();
                bloctoWalletProvider.ForceUseWebView = true;
            }
            catch (Exception e)
            {
                $"Init BloctoWalletProvider failure, Error message: {e.Message}".ToLog();
                throw;
            }
            
            return bloctoWalletProvider;
        }
        
        /// <summary>
        /// Get the wallet address
        /// </summary>
        /// <param name="callBack">Get wallet address, then execute instructions</param>
        public new void RequestAccount(Action<string> callBack)
        {
            base.RequestAccount(callBack);
            
            var parameters = new Dictionary<string, string>
                             {
                                 { "app_id", Uri.EscapeUriString(bloctoAppIdentifier.ToString())},
                                 { "blockchain", BloctoWalletProvider.chainName},
                                 { "method", ActionNameEnum.Request_Account.ToString().ToLower()},
                                 { "request_id", requestId.ToString()}
                             };
            
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(isInstalledApp && ForceUseWebView == false)
            {
                var appSb = GenerateUrl(appSdkDomain, parameters);
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb, ForceUseWebView));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            var webSb = GenerateUrl(webSdkDomain, parameters);
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb, ForceUseWebView));
        }
        
        /// <summary>
        /// Sign and send transaction
        /// </summary>
        /// <param name="fromAddress">Wallet address</param>
        /// <param name="transaction">Transaction data</param>
        /// <param name="callBack"></param>
        public void SignAndSendTransaction(string fromAddress, Transaction transaction, Action<string> callBack)
        {
            base.SendTransaction(callBack);
            var tmp = new TransactionBuilder().SetRecentBlockHash(transaction.RecentBlockHash)
                                                            .SetFeePayer(transaction.FeePayer);

            foreach (var instruction in transaction.Instructions)
            {
                tmp.AddInstruction(instruction);
            }
            
            var sendTransactionPreRequest = new SendTransactionPreRequest
                                            {
                                                From = fromAddress,
                                                IsInvokeWrapped = transaction.Instructions
                                                                             .Any(p => Base58Encoding.Encode(p.ProgramId) == BloctoWalletProvider.walletProgramId),
                                            };
            
            if(transaction.Signatures != null)
            {
                sendTransactionPreRequest.PublicKeySignaturePairs = transaction.Signatures
                                                                               .Where(p => p.Signature != null)
                                                                               .Distinct(p => p.PublicKey)
                                                                               .ToDictionary(pubKeyPair => pubKeyPair.PublicKey.Key, pubKeyPair => pubKeyPair.Signature.ToHex());
            }

            var tx = tmp.BuildExecludeSign();
            sendTransactionPreRequest.Message = tx.ToHex();
            $"Transaction message: {sendTransactionPreRequest.Message}".ToLog();
            $"Installed App: {isInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            
            if(isInstalledApp && ForceUseWebView == false)
            {
                var parameters = GetUrlParameters(sendTransactionPreRequest);
                var appSb = GenerateUrl(appSdkDomain, parameters);
                _appendTxDict.Clear();
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            $"WebSDK domain: {webSdkDomain}".ToLog();
            foreach (var item in _appendTxDict.SelectMany(appendTx => appendTx.Value.Select(item => item)))
            {
                sendTransactionPreRequest.AppenTxDict.Add(item.Key, item.Value);
            }
                
            // var webSb = GenerateUrl(webSdkDomain, parameters);
            var webSb = string.Empty;
            _appendTxDict.Clear();
            $"ForcedUseWebView: {ForceUseWebView}, Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb, ForceUseWebView));
        }

        /// <summary>
        /// Convert the transaction data back to an raw transaction
        /// </summary>
        /// <param name="address">Wallet address</param>
        /// <param name="transaction">Transaction data</param>
        /// <returns></returns>
        public Transaction ConvertToProgramWalletTransaction(string address, Transaction transaction)
        {
            var tmp = new TransactionBuilder().SetRecentBlockHash(transaction.RecentBlockHash).SetFeePayer(transaction.FeePayer);
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
            var messageBytes = response.RawTx.HexToBytes();
            var message = Message.Deserialize(messageBytes);
            
            var programTransaction = new Transaction
                                     {
                                         RecentBlockHash = message.RecentBlockhash
                                     };
            
            if(Convert.ToInt32(message.Header.RequiredSignatures) > 0)
            {
                programTransaction.FeePayer = message.AccountKeys[0];
            }

            // var accountKeys = new List<string>();
            foreach (var instruction in message.Instructions)
            {
                var keys = instruction.KeyIndices.Select(indices => {
                                                             var publicKey = message.AccountKeys[Convert.ToInt32(indices)];
                                                             var isSigner = Convert.ToInt32(indices) < Convert.ToInt32(message.Header.RequiredSignatures);
                                                             var accountMeta = new AccountMeta(publicKey, message.IsAccountWritable(Convert.ToInt32(indices)), isSigner);
                                                             return accountMeta;
                                                         }).ToList();
                // accountKeys.AddRange(keys.Select(p => p.PublicKey));
                var transactionInstruction = new TransactionInstruction
                                             {
                                                 ProgramId = message.AccountKeys[instruction.ProgramIdIndex],
                                                 Keys = keys,
                                                 Data = instruction.Data
                                             };
                
                programTransaction.Add(transactionInstruction);
            }
            
            var key = programTransaction.CompileMessage();
            _appendTxDict.Add(key.ToHex(), response.ExtraData.AppendData);
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
            if(requestIdActionMapper.ContainsKey(item.RequestId))
            {
                var action = requestIdActionMapper[item.RequestId];
                if(item.RemainContent.Contains("error="))
                {
                    var errorMessage = UniversalLinkHandler(item.RemainContent, "error=");
                    throw new Exception($"{errorMessage}");
                }
                
                switch (action)
                {
                    case "CONNECTWALLET":
                        var result = UniversalLinkHandler(item.RemainContent, "address=");
                        sessionId = UniversalLinkHandler(item.RemainContent, "session_id=");
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
        
        private Dictionary<string, string> GetUrlParameters(SendTransactionPreRequest sendTransactionPreRequest)
        {
            var right = Uri.EscapeDataString("[");
            var left = Uri.EscapeDataString("]");
            var parameters = new Dictionary<string, string>
                             {
                                 { "app_id", Uri.EscapeUriString(bloctoAppIdentifier.ToString()) },
                                 { "request_id", requestId.ToString() },
                                 { "method", ActionNameEnum.Sign_And_Send_Transaction.ToString().ToLower() },
                                 { "blockchain", BloctoWalletProvider.chainName },
                                 { "from", sendTransactionPreRequest.From },
                                 { "message", sendTransactionPreRequest.Message },
                                 { "is_invoke_wrapped", sendTransactionPreRequest.IsInvokeWrapped.ToString().ToLower() }
                             };

            foreach (var queryStr in sendTransactionPreRequest.PublicKeySignaturePairs.Select(pair => $"public_key_signature_pairs{right}{pair.Key}{left}={pair.Value}"))
            {
                var item = queryStr.Split("=");
                parameters.Add(item[0], item[1]);
            }

            foreach (var queryStr in _appendTxDict.SelectMany(appendTx => appendTx.Value.Select(item => $"append_tx{right}{item.Key}{left}={item.Value}")))
            {
                var item = queryStr.Split("=");
                parameters.Add(item[0], item[1]);
            }

            return parameters;
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