using System.Collections.Generic;

namespace Flow.FCL.Models
{
    public class AccountProofData
    {
        public AccountProofData()
        {
            Signature = new List<Signature>();
        }
        
        /// <summary>
        /// App Name
        /// </summary>
        public string AppId { get; set; }
        
        /// <summary>
        /// Random nonce as a hex string
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Signature
        /// </summary>
        public List<Signature> Signature { get; set; }
    }
}