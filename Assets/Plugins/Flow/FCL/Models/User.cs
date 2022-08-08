using System;
using Flow.Net.Sdk.Core.Models;

namespace Flow.FCL.Models
{
    public class User
    {
        public FlowAddress Addr { get; set; }

        public string Cid { get; set; }

        public decimal ExpiresAt { get; set; }

        public string F_type { get; set; }

        public string F_vsn { get; set; }

        public bool LoggedIn { get; set; }
        
    }
}