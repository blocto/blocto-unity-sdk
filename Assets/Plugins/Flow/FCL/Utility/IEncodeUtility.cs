namespace Flow.FCL.Utility
{
    public interface IEncodeUtility
    {
        public string GetEncodeMessage(string appIdentifier, string address, string nonce);
    }
}