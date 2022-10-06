using System.Collections.Generic;
using Flow.Net.Sdk.Core.Models;

namespace Flow.FCL.Models
{
    public class AccountProofData
    {
        public AccountProofData()
        {
            Signature = new List<FlowSignature>();
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
        public List<FlowSignature> Signature { get; set; }
    }
}
