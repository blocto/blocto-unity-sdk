using Solnet.Rpc.Messages;
using System.Text;
using Blocto.Sdk.Core.Utility;
using Blocto.Sdk.Solana.Converts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Networking;

namespace Solnet.Rpc.Core.Http
{
    /// <summary>
    /// Base Rpc client class that abstracts the HttpClient handling.
    /// </summary>
    public abstract class JsonRpcClient
    {
        private WebRequestUtility _webRequestUtility;
        
        private JsonSerializerSettings _jsonSerializerSettings;
        
        private string _url;
        protected JsonRpcClient(string url, WebRequestUtility webRequestUtility)
        {
            _url = url;
            _webRequestUtility = webRequestUtility;
            _jsonSerializerSettings = new JsonSerializerSettings
                                      {
                                          ContractResolver = new CamelCasePropertyNamesContractResolver()
                                      };
            _jsonSerializerSettings.Converters.Add(new EncodingConverter());
        }

        protected RequestResult<T> SendRequest<T>(JsonRpcRequest req)
        {
            var requestJson = JsonConvert.SerializeObject(req, _jsonSerializerSettings);
            var payload = Encoding.UTF8.GetBytes(requestJson);
            var uploadHandler = new UploadHandlerRaw(payload);
            var request = _webRequestUtility.CreateUnityWebRequest(_url, "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<JsonRpcResponse<T>>(request);
            var result = new RequestResult<T>(response);
            return result;
        }
    }

}