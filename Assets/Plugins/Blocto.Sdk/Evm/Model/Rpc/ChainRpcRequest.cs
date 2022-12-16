using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Blocto.Sdk.Evm.Model.Rpc
{
    [JsonObject]
    public class ChainRpcRequest
    {
        [JsonConstructor]
        private ChainRpcRequest()
        {

        }
		
        /// <param name="id">Request id</param>
        /// <param name="method">Target method name</param>
        /// <param name="parameterList">List of parameters for the target method</param>
        public ChainRpcRequest(object id, string method, params object[] parameterList)
        {
            this.Id = id;
            this.JsonRpcVersion =  "2.0";
            this.Method = method;
            this.RawParameters = parameterList;
            _parameterType = "string";
        }

        /// <param name="id">Request id</param>
        /// <param name="method">Target method name</param>
        /// <param name="parameterMap">Map of parameter name to parameter value for the target method</param>
        public ChainRpcRequest(object id, string method, Dictionary<string, object> parameterMap)
        {
            this.Id = id;
            this.JsonRpcVersion =  "2.0";
            this.Method = method;
            this.RawParameters = parameterMap;
            _parameterType = "map";
        }

        /// <summary>
        /// Request Id (Optional)
        /// </summary>
        [JsonProperty("id")]
        public object Id { get; private set; }
        /// <summary>
        /// Version of the JsonRpc to be used (Required)
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRpcVersion { get; private set; }
        /// <summary>
        /// Name of the target method (Required)
        /// </summary>
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; private set; }
        /// <summary>
        /// Parameters to invoke the method with (Optional)
        /// </summary>
        [JsonProperty("params")]
        [JsonConverter(typeof(RpcParametersJsonConverter))]
        public object RawParameters { get; private set; } 
        
        private string _parameterType;
        
        public string GetJsonStr()
        {
            BodyTemplate.Id = Id.ToString();
            BodyTemplate.MethodName = Method;
            var sb = new StringBuilder("");
            var content = string.Empty;
            if(_parameterType == "string")
            {
                foreach (var item in (object[])RawParameters)
                {
                    sb.Append($@"""{item}"",");
                }
                
                Debug.Log($"DEBUG SB string: {sb.ToString()}");
                if(sb.ToString().EndsWith(","))
                {
                    sb.Remove(sb.ToString().Length - 1, 1);
                }
                
                Debug.Log($"DEBUG SB string: {sb.ToString()}");
                content = sb.ToString();
            }
            
            BodyTemplate.Parameters = content;
            return BodyTemplate.BodyContent;
        }
    }
}