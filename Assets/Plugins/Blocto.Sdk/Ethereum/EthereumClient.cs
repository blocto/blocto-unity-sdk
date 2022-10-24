using System.Numerics;
using System.Text;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Ethereum.Model.Rpc;
using Blocto.Sdk.Ethereum.Utility;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Blocto.Sdk.Ethereum
{
    public class EthereumClient 
    {
        private WebRequestUtility _webRequestUtility;
        public EthereumClient(WebRequestUtility webRequestUtility)
        {
            _webRequestUtility = webRequestUtility;
        }
            
        public decimal GetAddressBalance(string address)
        {
            var baseDomain = "https://rinkeby.blocto.app";
            var request = new ChainRpcRequest(1, "eth_getBalance", address, "latest");
            var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            var uploadHandler = new UploadHandlerRaw(requestBytes); 
            var webRequest = _webRequestUtility.CreateUnityWebRequest(baseDomain, "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<RpcResponse>(webRequest);
            // var rpcClient = new UnityRpcClient<string>(new JsonSerializerSettings(), "https://rinkeby.infura.io/v3/f0c27d3cc51540179fe82727603d4400");
            // var request = new RpcRequest(1, "eth_getBalance", address, "latest");
            //
            // $"Call rpc client sendRequest: {DateTime.Now:HH:mm:ss.fff}".ToLog();
            //
            // // var result = rpcClient.SendRequestAsync(request).GetAwaiter().GetResult();
            // StartCoroutine(rpcClient.SendRequest(request));
            // while (rpcClient.Result is null)
            // {
            //     yield return new WaitForSeconds(0.2f);
            // }
            //
            var tmp = BigInteger.Parse(response.Result.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            var balance = EthConvert.FromWei(tmp); 
            return balance;
        }
        
    }
}