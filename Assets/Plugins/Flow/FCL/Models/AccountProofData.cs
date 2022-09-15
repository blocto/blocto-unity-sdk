using Flow.Net.Sdk.Core.Models;

namespace Plugins.Flow.FCL.Models
{
    public class AccountProofData
    {
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
        public Signature Signature { get; set; }
    }
}