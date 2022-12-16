namespace Blocto.Sdk.Evm.Model.Hex
{
    public interface IHexConvertor<T>
    {
        string ConvertToHex(T value);
        T ConvertFromHex(string value);
    }
}