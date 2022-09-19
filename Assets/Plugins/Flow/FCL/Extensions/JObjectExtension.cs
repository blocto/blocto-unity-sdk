using Newtonsoft.Json.Linq;

namespace Flow.FCL.Extensions
{
    public static class JObjectExtension
    {
        public static string SessionId(this JObject jobj) => jobj.GetValue("sessionId")?.ToString();
        
        public static string AuthorizationId(this JObject jobj) => jobj.GetValue("authorizationId")?.ToString();
        
        public static string SignatureStr(this JObject jobj) => jobj.GetValue("signature")?.ToString();
        
        public static string Address(this JObject jobj) => jobj.GetValue("addr")?.ToString();
        
        public static string KeyId(this JObject jobj) => jobj.GetValue("keyId")?.ToString();
    }
}