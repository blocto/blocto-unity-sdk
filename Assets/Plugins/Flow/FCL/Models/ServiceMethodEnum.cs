using System.Runtime.Serialization;

namespace Flow.FCL.Models
{
    public enum ServiceMethodEnum
    {
        [EnumMember(Value = "HTTP/POST")] 
        HTTPPOST,
        
        [EnumMember(Value = "DATA")] 
        DATA,
        
        [EnumMember(Value = "IFRAME/RPC")] 
        IFRAMERPC,
        
        [EnumMember(Value = "POP/RPC")] 
        POPRPC,
        
        [EnumMember(Value = "TAB/RPC")] 
        TABRPC,
        
        [EnumMember(Value = "EXT/RPC")] 
        EXTRPC,
    }
}