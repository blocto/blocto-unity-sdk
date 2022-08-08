using System.Collections.Generic;

namespace Flow.Net.Sdk.Utility
{
    public class RLPCollection : List<IRLPElement>, IRLPElement
    {
        public byte[] RLPData { get; set; }
    }
}