using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blocto.Sdk.Core.Extension;
using Blocto.Sdk.Core.Model;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Solana.Model;
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
        
        private string _walletProgramId = "Ckv4czD7qPmQvy2duKEa45WRp3ybD2XuaJzQAWrhAour";
        
        private const string chainName = "solana";
        
        private EnvEnum _env;

        private Action<string> _signMessageCallback;
       
        private Dictionary<string, Dictionary<string, string>> _appendTxDict;
        
        private Dictionary<string, Action<SendTransactionPreRequest>> _transactionProcessDict;

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
            try
            {
                bloctoWalletProvider.WebRequestUtility = gameObject.AddComponent<WebRequestUtility>();
                bloctoWalletProvider.WebRequestUtility.BloctoAppId = bloctoAppIdentifier.ToString();
                bloctoWalletProvider.BloctoAppIdentifier = bloctoAppIdentifier;
                bloctoWalletProvider.gameObject.name = "bloctowalletprovider";
                bloctoWalletProvider.IsCancelRequest = false;
                bloctoWalletProvider._env = env;
            
                switch (env)
                {
                    case EnvEnum.Devnet:
                        bloctoWalletProvider.BackedApiDomain = bloctoWalletProvider.BackedApiDomain.Replace("api", "api-dev");
                        bloctoWalletProvider.AndroidPackageName = $"{bloctoWalletProvider.AndroidPackageName}.dev";
                        bloctoWalletProvider.AppSdkDomain = bloctoWalletProvider.AppSdkDomain.Replace("blocto.app", "dev.blocto.app");
                        bloctoWalletProvider.WebSdkDomain = bloctoWalletProvider.WebSdkDomain.Replace("wallet.blocto.app", "wallet-dev.blocto.app");
                        bloctoWalletProvider._walletProgramId = "Ckv4czD7qPmQvy2duKEa45WRp3ybD2XuaJzQAWrhAour";
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.DevNet, bloctoWalletProvider.WebRequestUtility);
                        bloctoWalletProvider.WebSdkDomainV2 = bloctoWalletProvider.WebSdkDomainV2.Replace("wallet-v2.blocto.app", "wallet-v2-dev.blocto.app");
                        break;
                    case EnvEnum.Mainnet:
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.MainNet, bloctoWalletProvider.WebRequestUtility);
                        break;
                    case EnvEnum.Testnet:
                        bloctoWalletProvider.SolanaClient = ClientFactory.GetClient(Cluster.TestNet, bloctoWalletProvider.WebRequestUtility);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(env), env, null);
                }

                if(Application.platform == RuntimePlatform.Android)
                {
                    bloctoWalletProvider.InitializePlugins("com.blocto.unity.UtilityActivity");
                }
            
                bloctoWalletProvider.IsInstalledApp = bloctoWalletProvider.CheckInstalledApp(bloctoWalletProvider._env);
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

        public BloctoWalletProvider()
        {
            _transactionProcessDict = new Dictionary<string, Action<SendTransactionPreRequest>>
                                      {
                                          {"true_false", SignAndSendTransactionByApp},
                                          {"true_true", SignAndSendTransactionByWeb},
                                          {"false_true", SignAndSendTransactionByWeb},
                                          {"false_false", SignAndSendTransactionByWeb},
                                      };
        }
        
        /// <summary>
        /// Get the wallet address
        /// </summary>
        /// <param name="callBack">Get wallet address, then execute instructions</param>
        public new void RequestAccount(Action<string> callBack)
        {
            base.RequestAccount(callBack);
            
            
            
            $"Installed App: {IsInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            if(IsInstalledApp && ForceUseWebView == false)
            {
                var parameters = new Dictionary<string, string>
                                 {
                                     { "app_id", Uri.EscapeUriString(BloctoAppIdentifier.ToString())},
                                     { "blockchain", BloctoWalletProvider.chainName},
                                     { "method", ActionNameEnum.Request_Account.ToString().ToLower()},
                                     { "request_id", RequestId.ToString()}
                                 };
                
                var appSb = GenerateUrl(AppSdkDomain, parameters);
                
                $"Url: {appSb}".ToLog();
                StartCoroutine(OpenUrl(appSb));
                return;
            }
            
            $"WebSDK domain: {WebSdkDomain}".ToLog();
            var webSb = CreateRequestAccountUrlV2("solana", BloctoAppIdentifier.ToString());
            $"Url: {webSb}".ToLog();
            StartCoroutine(OpenUrl(webSb));
        }
        
        /// <summary>
        /// Sign and send transaction
        /// </summary>
        /// <param name="fromAddress">Wallet address</param>
        /// <param name="transaction">Transaction data</param>
        /// <param name="callBack"></param>
        public void SignAndSendTransaction(string fromAddress, Transaction transaction, Action<string> callBack)
        {
            base.SendTransaction(callBack, "SIGNANDSENETRANSACTION");
            var tmp = new TransactionBuilder().SetRecentBlockHash(transaction.RecentBlockHash)
                                                            .SetFeePayer(transaction.FeePayer);

            foreach (var instruction in transaction.Instructions)
            {
                tmp.AddInstruction(instruction);
            }
            
            var sendTransactionPreRequest = new SendTransactionPreRequest(fromAddress, transaction, _appendTxDict)
                                            {
                                                IsInvokeWrapped = transaction.Instructions.Any(p => Base58Encoding.Encode(p.ProgramId) == _walletProgramId),
                                                Message = tmp.BuildExecludeSign().ToHex()
                                            };
            _appendTxDict.Clear();
            
            $"Installed App: {IsInstalledApp}, ForceUseWebView: {ForceUseWebView}".ToLog();
            var process = _transactionProcessDict[$"{IsInstalledApp}_{ForceUseWebView}".ToLower()];
            process.Invoke(sendTransactionPreRequest);
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
            
            var request = new CreateRawTxRequest
                          {
                              Address = address,
                              RawTx = tmp.BuildExecludeSign().Select(b => (sbyte)b).ToArray().ToHex()
                          };
            
            var url = $"{BackedApiDomain}/{BloctoWalletProvider.chainName}/createRawTransaction";
            var response = WebRequestUtility.GetResponse<CreateRawTxResponse>(url, "POST", "application/json", request);
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

            foreach (var instruction in message.Instructions)
            {
                var transactionInstruction = new TransactionInstruction
                                             {
                                                 ProgramId = message.AccountKeys[instruction.ProgramIdIndex],
                                                 Keys = instruction.KeyIndices.Select(indices => {
                                                                                          var publicKey = message.AccountKeys[Convert.ToInt32(indices)];
                                                                                          var isSigner = Convert.ToInt32(indices) < Convert.ToInt32(message.Header.RequiredSignatures);
                                                                                          var accountMeta = new AccountMeta(publicKey, message.IsAccountWritable(Convert.ToInt32(indices)), isSigner);
                                                                                          return accountMeta;
                                                                                      })
                                                                               .ToList(),
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
            if (!RequestIdActionMapper.TryGetValue(item.RequestId, out var action))
            {
                return;
            }
            
            if(item.RemainContent.Contains("error="))
            {
                var errorMessage = UniversalLinkHandler(item.RemainContent, "error=");
                throw new Exception($"{errorMessage}");
            }
                
            $"Action: {action}".ToLog();
            switch (action)
            {
                case "CONNECTWALLET":
                    var result = UniversalLinkHandler(item.RemainContent, "address=");
                    SessionId = UniversalLinkHandler(item.RemainContent, "session_id=");
                    ConnectWalletCallback.Invoke(result);
                    break;
                    
                case "SIGNANDSENETRANSACTION":
                    $"In Sign and SendTransaction, remain content: {item.RemainContent}".ToLog();
                    var signature = UniversalLinkHandler(item.RemainContent, "tx_hash=");
                    SendTransactionCallback.Invoke(signature);
                    break;
            }
        }
        
        private void SignAndSendTransactionByWeb(SendTransactionPreRequest sendTransactionPreRequest)
        {
            "Send by Web".ToLog();
            
            var transactionPreResponse = SendData<SendTransactionPreRequest, TransactionPreResponse>(BloctoWalletProvider.chainName, AuthzApiUrl, sendTransactionPreRequest);
            var webSb = AuthzWebUrl(transactionPreResponse.AuthorizationId, BloctoWalletProvider.chainName);
            $"Url: {webSb}".ToLog();

            StartCoroutine(OpenUrl(webSb.ToString()));
        }

        private void SignAndSendTransactionByApp(SendTransactionPreRequest sendTransactionPreRequest)
        {
            "Send by App".ToLog();
            var parameters = GetUrlParameters(sendTransactionPreRequest);
            var appSb = GenerateUrl(AppSdkDomain, parameters);

            $"Url: {appSb}".ToLog();
            StartCoroutine(OpenUrl(appSb));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendTransactionPreRequest"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetUrlParameters(SendTransactionPreRequest sendTransactionPreRequest)
        {
            var right = Uri.EscapeDataString("[");
            var left = Uri.EscapeDataString("]");
            var parameters = new Dictionary<string, string>
                             {
                                 { "app_id", Uri.EscapeUriString(BloctoAppIdentifier.ToString()) },
                                 { "request_id", RequestId.ToString() },
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