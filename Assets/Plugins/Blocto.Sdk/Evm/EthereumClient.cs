using System.Dynamic;
using System.Numerics;
using System.Text;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm.Model.Rpc;
using Blocto.Sdk.Evm.Utility;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Blocto.Sdk.Evm
{
    public class EthereumClient 
    {
        private const string ERC1271_MAGIC_VALUE = "0x1626ba7e00000000000000000000000000000000000000000000000000000000";
        
        private WebRequestUtility _webRequestUtility;
        
        private string _baseDomain;
        public EthereumClient(string baseDomain, WebRequestUtility webRequestUtility)
        {
            _baseDomain = baseDomain;
            _webRequestUtility = webRequestUtility;
        }
            
        public decimal GetAddressBalance(string address)
        {
            var request = new ChainRpcRequest(1, "eth_getBalance", address, "latest");
            var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            var uploadHandler = new UploadHandlerRaw(requestBytes); 
            var webRequest = _webRequestUtility.CreateUnityWebRequest(_baseDomain, "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<RpcResponse>(webRequest);
            var tmp = BigInteger.Parse(response.Result.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            var balance = EthConvert.FromWei(tmp); 
            return balance;
        }
        
        public bool IsValidSignature(string address, CallInput data)
        {
            var request = new ChainRpcRequest(1, "eth_call", data, "latest");
            var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            var uploadHandler = new UploadHandlerRaw(requestBytes); 
            var webRequest = _webRequestUtility.CreateUnityWebRequest(_baseDomain, "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<RpcResponse>(webRequest);
            var result = response.Result;
            
            return result.ToString() == EthereumClient.ERC1271_MAGIC_VALUE ? true : false;
        }
    }
}