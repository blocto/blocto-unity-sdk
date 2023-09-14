using System;
using System.Linq;
using System.Text.RegularExpressions;
using Blocto.Sdk.Core.Extension;

namespace Blocto.Sdk.Core.Model
{
    public static class UniversalLinkExtensions
    {
        public static (string RequestId, string RemainContent) RequestId(this string link)
        {
            var elements = link.Split("&");
            var element = elements.FirstOrDefault(p => p.Contains("request_id"));
            var remainStr = default(string);
            if (element is null)
            {
                remainStr = string.Join("&", elements);
                return (Guid.Empty.ToString(), remainStr);
            }

            var id = element.Split("=")[1];
            var tmp = elements.ToList();
            tmp.Remove(element);
            remainStr = string.Join("&", tmp);
            return (id, remainStr);
        }

		public static (int Index, string Name, string Value) AddressParser(this string text)
        {
            var value = text.Split("=")[1];
            return (0, "address", value);
        }
        
        public static (int Index, string Name, string Value) SignatureParser(this string text)
        {
            var keyValue = text.Split("=");
            var propertiesPattern = @"(?<=\[)(.*)(?=\])";

            var match = Regex.Match(keyValue[0], propertiesPattern);
            if (!match.Success)
            {
                throw new Exception("App sdk return value format error");
            }

            var elements = match.Captures.FirstOrDefault()?.Value.Split("][");
            return (Convert.ToInt32(elements?[0]), elements?[1], keyValue[1]);
        }
    }
}