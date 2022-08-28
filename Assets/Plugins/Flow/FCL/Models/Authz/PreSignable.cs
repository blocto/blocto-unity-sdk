using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class PreSignable : BaseSignable
    {
        public PreSignable()
        {
            F_Type = "PreSignable";
            F_Vsn = "1.0.1";
            // Data = new object();
        }
        
        [JsonProperty("f_type")]
        public string F_Type { get; set; }
        
        [JsonProperty("f_vsn")]
        public string F_Vsn { get; set; }
        
        [JsonProperty("roles")]
        public object Roles { get; set; }
        
        [JsonProperty("cadence")]
        public string Cadence { get; set; }
        
        [JsonProperty("args")]
        public List<BaseArgument> Args { get; set; }
        
        [JsonProperty("interaction")]
        public Interaction Interaction { get; set; }
        
        [JsonProperty("voucher")]
        public Voucher Voucher { get; set; }
        
        [JsonProperty("data")]
        public object Data { get; set; }
        
        public virtual PreSignable DeepCopy()
        {
            var temp = (PreSignable) this.MemberwiseClone();
            return temp;
        }
    }
}