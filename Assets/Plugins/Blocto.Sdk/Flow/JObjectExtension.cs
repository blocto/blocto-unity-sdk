using Newtonsoft.Json.Linq;

namespace Blocto.Flow
{
    public static class JObjectExtension
    {
        public static string SessionId(this JObject jobj) => jobj.GetValue("sessionId")?.ToString();
        
        public static string AuthorizationId(this JObject jobj) => jobj.GetValue("authorizationId")?.ToString();
        
        public static string SignatureStr(this JObject jobj) => jobj.GetValue("signaturestr")?.ToString();
        
        public static string KeyId(this JObject jobj) => jobj.GetValue("keyId")?.ToString();
    }
}