using System.ComponentModel;

namespace Blocto.Sdk.Evm.Model
{
    public enum SignTypeEnum
    {
        [Description("sign")]
        Eth_Sign,
        
        [Description("personal_sign")]
        Personal_Sign,
        
        [Description("typed_data_sign")]
        SignTypedData,
        
        [Description("typed_data_sign_v3")]
        SignTypedDataV3,
        
        [Description("typed_data_sign_v4")]
        SignTypedDataV4,
    }
}