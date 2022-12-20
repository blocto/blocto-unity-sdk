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

            //UnityEngine.Debug.Log($"\tRequest: {requestJson}");
            // HttpResponseMessage response = null;
            // RequestResult<T> result = null;

            var payload = Encoding.UTF8.GetBytes(requestJson);
            var uploadHandler = new UploadHandlerRaw(payload);
            var request = _webRequestUtility.CreateUnityWebRequest(_url, "POST", "application/json", new DownloadHandlerBuffer(), uploadHandler);
            var response = _webRequestUtility.ProcessWebRequest<JsonRpcResponse<T>>(request);
            var result = new RequestResult<T>(response);
            return result;
            
            // using (var request = new UnityWebRequest(url, "POST"))
            // {
            //     byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestJson);
            //     request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            //     request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //     request.SetRequestHeader("Content-Type", "application/json");
            //
            //     //Send the request then wait here until it returns
            //     request.SendWebRequest();
            //
            //     if (request.result == UnityWebRequest.Result.ConnectionError)
            //     {
            //         UnityEngine.Debug.Log("Error While Sending: " + request.error);
            //         //result = new RequestResult<T>(400, request.error.ToString());
            //     }
            //     else
            //     {
            //         while (!request.isDone)
            //         {
            //             //UnityEngine.Debug.Log("Delay");
            //             await Task.Yield();
            //         }
            //
            //         var res = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(request.downloadHandler.text);
            //         result = new RequestResult<T>(response);
            //         if (res.Result != null)
            //         {
            //             result.Result = res.Result;
            //         }
            //         else
            //         {
            //             var errorRes = JsonUtility.FromJson<JsonRpcErrorResponse>("");
            //             if (errorRes != null && errorRes.Error != null)
            //             {
            //                 result.Reason = errorRes.Error.Message;
            //                 result.ServerErrorCode = errorRes.Error.Code;
            //             }
            //             else
            //             {
            //                 result.Reason = "Something wrong happened.";
            //             }
            //         }
            //     }
            // }
          
        }

    }

}