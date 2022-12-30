using System.Numerics;
using System.Text;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Evm.Model.Rpc;
using Blocto.Sdk.Evm.Utility;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Blocto.Sdk.Evm
{
    public class EthereumClient 
    {
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
        
    }
}