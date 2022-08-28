using Newtonsoft.Json;

namespace Flow.FCL.Models.Authz
{
    public class Signable : PreSignable
    {
        public Signable()
            : base()
        {
            F_Type = "Signable";
            // Data = new object();
        }
        
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("addr")]
        public string Addr { get; set; }
        
        [JsonProperty("keyId")]
        public int KeyId { get; set; }
        
        public new virtual Signable DeepCopy()
        {
            var temp = (Signable) this.MemberwiseClone();
            return temp;
        }
    }
}