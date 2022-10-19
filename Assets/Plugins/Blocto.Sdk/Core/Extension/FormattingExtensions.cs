using System;
using System.Globalization;

namespace Blocto.Sdk.Core.Extension
{
    public static class FormattingExtensions
    {
        public static string ToStringInvariant<T>(this T formattable) where T : IFormattable
        {
            if (formattable == null) throw new ArgumentNullException(nameof(formattable));

            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }
    }
}