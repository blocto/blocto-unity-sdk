using System.Collections.Generic;
using System.Linq;
using Blocto.Sdk.Core.Extension;
using Newtonsoft.Json;
using Solnet.Rpc.Models;
using Solnet.Wallet.Utilities;

namespace Blocto.Sdk.Solana.Model
{
    public class SendTransactionPreRequest
    {
        public SendTransactionPreRequest()
        {
            PublicKeySignaturePairs = new Dictionary<string, string>();
            AppenTxDict = new Dictionary<string, string>();
        }

        /// <summary>
        /// SendTransactionPreRequest
        /// </summary>
        /// <param name="fromAddress">Address</param>
        /// <param name="transaction">Transaction data</param>
        /// <param name="appendTxs">Append txs</param>
        public SendTransactionPreRequest(string fromAddress, Transaction transaction, Dictionary<string, Dictionary<string, string>> appendTxs)
        {
            PublicKeySignaturePairs = new Dictionary<string, string>();
            AppenTxDict = new Dictionary<string, string>();
            
            From = fromAddress;
            SetAppendTx(appendTxs);
            SetPublicKeySignaturePairs(transaction);
        }
        
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("isInvokeWrapped")]
        public bool IsInvokeWrapped { get; set; }
        
        [JsonProperty("publicKeySignaturePairs")]
        public Dictionary<string, string> PublicKeySignaturePairs { get; set; }
        
        [JsonProperty("appendTx")]
        public Dictionary<string, string> AppenTxDict { get; set; }
        
        /// <summary>
        /// Set append tx
        /// </summary>
        /// <param name="appendTxs">Append txs</param>
        private void SetAppendTx(Dictionary<string, Dictionary<string, string>> appendTxs)
        {
            foreach (var item in appendTxs.SelectMany(appendTx => appendTx.Value.Select(item => item)))
            {
                AppenTxDict.Add(item.Key, item.Value);
            }
        }
        
        /// <summary>
        /// Set public key signature pairs
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        private void SetPublicKeySignaturePairs(Transaction transaction)
        {
            if(transaction.Signatures != null)
            {
                PublicKeySignaturePairs = transaction.Signatures
                                                          .Where(p => p.Signature != null)
                                                          .Distinct(p => p.PublicKey)
                                                          .ToDictionary(pubKeyPair => pubKeyPair.PublicKey.Key, pubKeyPair => pubKeyPair.Signature.ToHex());
            }
        }
    }
}