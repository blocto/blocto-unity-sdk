using System.Collections.Generic;
using Blocto.Sdk.Core.Utility;
using Flow.Net.SDK.Client.Unity.Unity;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Flow.Net.SDK.Extensions;
using UnityEngine;

namespace Flow.FCL.Utility
{
    public class AppUtils
    {
        private readonly string _verifyAccountProofScript = @"import FCLCrypto from {address} pub fun main( address: Address, message: String, keyIndices: [Int], signatures: [String] ): Bool { return FCLCrypto.verifyAccountProofSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures) }";
        private readonly string _verifyUserSignatureScript = @"import FCLCrypto from {address} pub fun main( address: Address, message: String, keyIndices: [Int], signatures: [String] ): Bool { return FCLCrypto.verifyUserSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures) }";
        private IFlowClient _flowClient;

        public AppUtils(IFlowClient flowClient)
        {
            _flowClient = flowClient;
            var env = FlowClientLibrary.Config.Get("flow.network");
            var contractAddress = FlowClientLibrary.Config.Get($"{env}.fclcrypto");
            _verifyAccountProofScript = _verifyAccountProofScript.Replace("{address}", contractAddress);
            _verifyUserSignatureScript = _verifyUserSignatureScript.Replace("{address}", contractAddress);
        }
        
        public AppUtils(GameObject gameObject)
        {
            _flowClient = new FlowUnityWebRequest(gameObject, FlowClientLibrary.Config.Get("accessNode.api")); 
            
            var env = FlowClientLibrary.Config.Get("flow.network");
            var contractAddress = FlowClientLibrary.Config.Get($"{env}.fclcrypto");
            _verifyAccountProofScript = _verifyAccountProofScript.Replace("{address}", contractAddress);
            _verifyUserSignatureScript = _verifyUserSignatureScript.Replace("{address}", contractAddress);
        }
        
        public bool VerifyUserSignatures(string message, string address, string keyId, string signature)
        {
            $"Account proof message: {message}, signature: {signature}".ToLog();
            
            var signatures = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceString(signature)
                });
            
            var signatureIndexes = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceNumber(CadenceNumberType.Int, keyId)
                });
            
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                new FlowScript
                {
                    Script = _verifyUserSignatureScript,
                    Arguments = new List<ICadence>
                                {
                                    new CadenceAddress(address),
                                    new CadenceString(message.StringToHex()),
                                    signatureIndexes,
                                    signatures
                                }
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            
            return response.As<CadenceBool>().Value;
            return true;
        }
        
        public bool VerifyAccountProofSignature(string appIdentifier, string address, string keyId, string nonce, string signature)
        {
            var message = RLP.GetEncodeMessage(appIdentifier, address, nonce);
            $"Account proof message: {message}, nonce: {nonce}, signature: {signature}".ToLog();
            
            var signatures = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceString(signature)
                });
            
            var signatureIndexes = new CadenceArray(
                new List<ICadence>
                {
                    new CadenceNumber(CadenceNumberType.Int, keyId)
                });
            
            var response = _flowClient.ExecuteScriptAtLatestBlockAsync(
                               new FlowScript
                               {
                                   Script = _verifyAccountProofScript,
                                   Arguments = new List<ICadence>
                                               {
                                                   new CadenceAddress(address),
                                                   new CadenceString(message),
                                                   signatureIndexes,
                                                   signatures
                                               }
                               }).ConfigureAwait(false).GetAwaiter().GetResult();
            
            return response.As<CadenceBool>().Value;
        }
    }
}