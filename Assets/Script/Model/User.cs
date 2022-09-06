using Flow.Net.Sdk.Core.Models;

namespace Script.Model
{
    public class User
    {
        public FlowAddress Address { get; set; }

        public ulong Balance { get; set; }

        public string Name { get; set; }
    }
}