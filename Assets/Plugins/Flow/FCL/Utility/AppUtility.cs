using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flow.FCL.Models;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL.Utility
{
    public class AppUtility
    {
        private string _verifyAccountProofScript = @"import FCLCrypto from {address} pub fun main( address: Address, message: String, keyIndices: [Int], signatures: [String] ): Bool { return FCLCrypto.verifyAccountProofSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures) }";
        private string _verifyUserSignatureScript = @"import FCLCrypto from {address} pub fun main( address: Address, message: String, keyIndices: [Int], signatures: [String] ): Bool { return FCLCrypto.verifyUserSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures) }";
        
        private IFlowClient _flowClient;
        
        private IEncodeUtility _encodeUtility;
        
        public AppUtility(IFlowClient flowClient)
        {
            _flowClient = flowClient;
            var env = FlowClientLibrary.Config.Get("flow.network");
            var contractAddress = FlowClientLibrary.Config.Get($"{env}.fclcrypto");
        }
        
        public AppUtility(GameObject gameObject, IEncodeUtility encodeUtility)
        {
            _flowClient = new FlowUnityWebRequest(gameObject, FlowClientLibrary.Config.Get("accessNode.api")); 
            _encodeUtility = encodeUtility;
        }
        
        public bool VerifyUserSignatures(string message, List<FlowSignature> flowSignatures, string fclCryptoContract)
        {
            _verifyUserSignatureScript = _verifyUserSignatureScript.Replace("{address}", fclCryptoContract);
            // var signatureStrs = flowSignatures.Select(p => new CadenceString(Encoding.UTF8.GetString(p.Signature))).Cast<ICadence>().ToList();
            // var signatures = new CadenceArray(signatureStrs);
            // var signatureIndexs = flowSignatures.Select(p => new CadenceNumber(CadenceNumberType.Int, p.KeyId.ToString())).Cast<ICadence>().ToList(); 
            // var signatureIndexes = new CadenceArray(signatureIndexs);
            
            
            var argumentSignatures = new List<ICadence>();
            var argumentIndex = new List<ICadence>();
            foreach (var flowSignature in flowSignatures)
            {
                argumentSignatures.Add(new CadenceString(Encoding.UTF8.GetString(flowSignature.Signature)));
                argumentIndex.Add(new CadenceNumber(CadenceNumberType.Int, flowSignature.KeyId.ToString()));
                
                $"KeyId: {flowSignature.KeyId}, Signature: {Encoding.UTF8.GetString(flowSignature.Signature)}".ToLog();
            }
            
            $"message: {message}, hexmessage: {message.StringToHex()}, in AppUtility".ToLog();
            var signatures = new CadenceArray(argumentSignatures);
            var signatureIndexes = new CadenceArray(argumentIndex);
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                new FlowScript
                {
                    Script = _verifyUserSignatureScript,
                    Arguments = new List<ICadence>
                                {
                                    new CadenceAddress(flowSignatures.First().Address.Address.AddHexPrefix()),
                                    new CadenceString(message.StringToHex()),
                                    signatureIndexes,
                                    signatures
                                }
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            
            return response.As<CadenceBool>().Value;
        }
        
        /// <summary>
        /// Verify account proof signature
        /// </summary>
        /// <param name="appIdentifier"></param>
        /// <param name="accountProofData"></param>
        /// <param name="fclCryptoContract"></param>
        /// <returns></returns>
        public bool VerifyAccountProofSignature(string appIdentifier, AccountProofData accountProofData, string fclCryptoContract)
        {
            _verifyAccountProofScript = _verifyAccountProofScript.Replace("{address}", fclCryptoContract);
            var message = _encodeUtility.GetEncodeMessage(appIdentifier, accountProofData.Signature.First().Address.Address, accountProofData.Nonce);
            
            var signatureStrs = accountProofData.Signature.Select(p => new CadenceString(Encoding.UTF8.GetString(p.Signature))).Cast<ICadence>().ToList();
            var signatures = new CadenceArray(signatureStrs);
            
            var signatureIndexs = accountProofData.Signature.Select(p => new CadenceNumber(CadenceNumberType.Int, p.KeyId.ToString())).Cast<ICadence>().ToList();
            var signatureIndexes = new CadenceArray(signatureIndexs);
            
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                               new FlowScript
                               {
                                   Script = _verifyAccountProofScript,
                                   Arguments = new List<ICadence>
                                               {
                                                   new CadenceAddress(accountProofData.Signature.First().Address.Address),
                                                   new CadenceString(message),
                                                   signatureIndexes,
                                                   signatures
                                               }
                               }).ConfigureAwait(false).GetAwaiter().GetResult();
            
            return response.As<CadenceBool>().Value;
        }
    }
}