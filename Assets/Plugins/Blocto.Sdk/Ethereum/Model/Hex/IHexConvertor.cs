namespace Blocto.Sdk.Ethereum.Model.Hex
{
    public interface IHexConvertor<T>
    {
        string ConvertToHex(T value);
        T ConvertFromHex(string value);
    }
}