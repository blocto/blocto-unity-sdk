using Flow.Net.Sdk.Core.Cadence;

namespace Flow.FCL.Models
{
    public class QueryResult
    {
        public ICadence Data { get; set; }

        public bool IsSuccessed { get; set; }

        public string Message { get; set; }
    }
}