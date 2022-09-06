using System;
using System.Collections.Generic;
using Flow.Net.Sdk.Core.Cadence;

namespace Flow.FCL
{
    public static class ICadenceExtension
    {
        public static TResult ConvertTo<TResult>(this ICadence cadence, Type type)
        {
            var reuslt = Activator.CreateInstance<TResult>();
            var properties = type.GetProperties();

            
            return default(TResult);
        }
    }
}