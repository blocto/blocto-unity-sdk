using System.Runtime.Serialization;

namespace Flow.FCL.Models
{
    public enum ServiceTypeEnum
    {
        [EnumMember(Value = "authn")] 
        AUTHN,
        
        [EnumMember(Value = "authz")] 
        AUTHZ,
        
        [EnumMember(Value = "pre-authz")] 
        PREAUTHZ,
        
        [EnumMember(Value = "user-signature")] 
        USERSIGNATURE,
        
        [EnumMember(Value = "open-id")] 
        OPENID,
        
        [EnumMember(Value = "account-proof")] 
        AccountProof
    }
}