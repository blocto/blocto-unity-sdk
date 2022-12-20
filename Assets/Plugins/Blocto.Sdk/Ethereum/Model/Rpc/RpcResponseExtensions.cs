using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Blocto.Sdk.Ethereum.Model.Rpc
{
    public static class RpcResponseExtensions
    {
        /// <summary>
        /// Parses and returns the result of the rpc response as the type specified. 
        /// Otherwise throws a parsing exception
        /// </summary>
        /// <typeparam name="T">Type of object to parse the response as</typeparam>
        /// <param name="response">Rpc response object</param>
        /// <param name="returnDefaultIfNull">Returns the type's default value if the result is null. Otherwise throws parsing exception</param>
        /// <returns>Result of response as type specified</returns>
        public static T GetResult<T>(this RpcResponse response, bool returnDefaultIfNull = true, JsonSerializerSettings settings = null)
        {
            Debug.Log($"DEBUG In GetResult(), {DateTime.Now:HH:mm:ss.fff}");
            if (response.Result == null)
            {
                if(!returnDefaultIfNull && default(T) != null)
                {
                    throw new Exception("Unable to convert the result (null) to type " + typeof(T));
                }
                return default(T);
            }
            try
            {
                if(settings == null)
                {
                    var result = response.Result.ToObject<T>();
                    Debug.Log($"DEBUG In GetResult-return(), {DateTime.Now:HH:mm:ss.fff}");
                    return result;
                }
                else
                {
                    var jsonSerializer = JsonSerializer.Create(settings);
                    var result = response.Result.ToObject<T>(jsonSerializer);
                    Debug.Log($"DEBUG In GetResult-return(), {DateTime.Now:HH:mm:ss.fff}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to convert the result to type " +  typeof(T), ex);
            }
        }
    }
}