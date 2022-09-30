using System;
using System.Linq;

namespace Blocto.Sdk.Flow.Model
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
    }
}