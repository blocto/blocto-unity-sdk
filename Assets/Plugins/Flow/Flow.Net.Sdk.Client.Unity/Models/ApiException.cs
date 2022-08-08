using System;
using System.Collections.Generic;
// ReSharper disable TooManyDependencies

namespace Flow.Net.SDK.Client.Unity.Models.Enums
{
    public partial class ApiException : Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + response.Substring(0, response.Length >= 512 ? 512 : response.Length), innerException)
        {
            StatusCode = statusCode;
            Response = response;
        }

        public override string ToString()
        {
            return $"HTTP Response: \n\n{Response}\n\n{base.ToString()}";
        } 
    }
    
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, TResult result, Exception innerException)
            : base(message, statusCode, response, innerException) =>
            Result = result;
    }
}