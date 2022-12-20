using System;
using System.Numerics;
using Blocto.Sdk.Core.Extension;

namespace Blocto.Sdk.Ethereum.Utility
{
    public class EthConvert
    {
        public static decimal FromWei(BigInteger value, EthUnitEnum unit = EthUnitEnum.Ether)
        {
            return EthConvert.FromWei(value, EthConvert.GetEthUnitValue(unit));
        }
        
        public static BigInteger ToWei(BigDecimal amount, EthUnitEnum fromUnit = EthUnitEnum.Ether)
        {
            return ToWeiFromUnit(amount, GetEthUnitValue(fromUnit));
        }
        
        public static BigInteger ToWei(decimal amount, EthUnitEnum fromUnit = EthUnitEnum.Ether)
        {
            return ToWeiFromUnit(amount, GetEthUnitValue(fromUnit));
        }
        
        public static BigInteger ToWeiFromUnit(decimal amount, BigInteger fromUnit)
        {
            return ToWeiFromUnit((BigDecimal) amount, fromUnit);
        }
        
        public static BigInteger ToWeiFromUnit(BigDecimal amount, BigInteger fromUnit)
        {
            TryValidateUnitValue(fromUnit);
            var bigDecimalFromUnit = new BigDecimal(fromUnit, 0);
            var conversion = amount * bigDecimalFromUnit;
            return conversion.Floor().Mantissa;
        }
        
        public static bool TryValidateUnitValue(BigInteger ethUnit)
        {
            if (ethUnit.ToStringInvariant().Trim('0') == "1") return true;
            throw new Exception("Invalid unit value, it should be a power of 10 ");
        }
    
        /// <summary>
        ///     Converts from wei to a unit, NOTE: When the total number of digits is bigger than 29 they will be rounded the less
        ///     significant digits
        /// </summary>
        private static decimal FromWei(BigInteger value, int decimalPlacesToUnit)
        {
            return (decimal) new BigDecimal(value, decimalPlacesToUnit * -1);
        }
    
        private static int GetEthUnitValueLength(BigInteger unitValue)
        {
            return unitValue.ToStringInvariant().Length - 1;
        }

        private static decimal FromWei(BigInteger value, BigInteger toUnit)
        {
            return EthConvert.FromWei(value, EthConvert.GetEthUnitValueLength(toUnit));
        }

        private static BigInteger GetEthUnitValue(EthUnitEnum ethUnitEnum)
        {
            return ethUnitEnum switch
                   {
                       EthUnitEnum.Wei => BigIntegerExtensions.ParseInvariant("1"),
                       EthUnitEnum.Kwei => BigIntegerExtensions.ParseInvariant("1000"),
                       EthUnitEnum.Babbage => BigIntegerExtensions.ParseInvariant("1000"),
                       EthUnitEnum.Femtoether => BigIntegerExtensions.ParseInvariant("1000"),
                       EthUnitEnum.Mwei => BigIntegerExtensions.ParseInvariant("1000000"),
                       EthUnitEnum.Picoether => BigIntegerExtensions.ParseInvariant("1000000"),
                       EthUnitEnum.Gwei => BigIntegerExtensions.ParseInvariant("1000000000"),
                       EthUnitEnum.Shannon => BigIntegerExtensions.ParseInvariant("1000000000"),
                       EthUnitEnum.Nanoether => BigIntegerExtensions.ParseInvariant("1000000000"),
                       EthUnitEnum.Nano => BigIntegerExtensions.ParseInvariant("1000000000"),
                       EthUnitEnum.Szabo => BigIntegerExtensions.ParseInvariant("1000000000000"),
                       EthUnitEnum.Microether => BigIntegerExtensions.ParseInvariant("1000000000000"),
                       EthUnitEnum.Micro => BigIntegerExtensions.ParseInvariant("1000000000000"),
                       EthUnitEnum.Finney => BigIntegerExtensions.ParseInvariant("1000000000000000"),
                       EthUnitEnum.Milliether => BigIntegerExtensions.ParseInvariant("1000000000000000"),
                       EthUnitEnum.Milli => BigIntegerExtensions.ParseInvariant("1000000000000000"),
                       EthUnitEnum.Ether => BigIntegerExtensions.ParseInvariant("1000000000000000000"),
                       EthUnitEnum.Kether => BigIntegerExtensions.ParseInvariant("1000000000000000000000"),
                       EthUnitEnum.Grand => BigIntegerExtensions.ParseInvariant("1000000000000000000000"),
                       EthUnitEnum.Einstein => BigIntegerExtensions.ParseInvariant("1000000000000000000000"),
                       EthUnitEnum.Mether => BigIntegerExtensions.ParseInvariant("1000000000000000000000000"),
                       EthUnitEnum.Gether => BigIntegerExtensions.ParseInvariant("1000000000000000000000000000"),
                       EthUnitEnum.Tether => BigIntegerExtensions.ParseInvariant("1000000000000000000000000000000"),
                       EthUnitEnum.Ada => default,
                       _ => throw new NotImplementedException()
                   };
        }
    }
}