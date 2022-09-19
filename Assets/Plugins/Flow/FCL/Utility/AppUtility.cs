using System.Collections.Generic;
using System.Text;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Plugins.Flow.FCL.Models;
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
        
        public bool VerifyUserSignatures(string message, FlowSignature signature, string fclCryptoContract)
        {
            _verifyUserSignatureScript = _verifyUserSignatureScript.Replace("{address}", fclCryptoContract);
            var signatures = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceString(Encoding.UTF8.GetString(signature.Signature))
                });
            
            var signatureIndexes = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceNumber(CadenceNumberType.Int, signature.KeyId.ToString())
                });
            
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                new FlowScript
                {
                    Script = _verifyUserSignatureScript,
                    Arguments = new List<ICadence>
                                {
                                    new CadenceAddress(signature.Address.Address.AddHexPrefix()),
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
            var message = _encodeUtility.GetEncodeMessage(appIdentifier, accountProofData.Signature.Addr, accountProofData.Nonce);
            
            var signatures = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceString(accountProofData.Signature.SignatureStr)
                });
            
            var signatureIndexes = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceNumber(CadenceNumberType.Int, accountProofData.Signature.KeyId.ToString())
                });
            
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                               new FlowScript
                               {
                                   Script = _verifyAccountProofScript,
                                   Arguments = new List<ICadence>
                                               {
                                                   new CadenceAddress(accountProofData.Signature.Addr),
                                                   new CadenceString(message),
                                                   signatureIndexes,
                                                   signatures
                                               }
                               }).ConfigureAwait(false).GetAwaiter().GetResult();
            
            return response.As<CadenceBool>().Value;
        }
    }
}