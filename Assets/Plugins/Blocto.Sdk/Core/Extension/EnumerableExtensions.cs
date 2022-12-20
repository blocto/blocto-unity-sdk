using System;
using System.Collections.Generic;
using System.Linq;

namespace Blocto.Sdk.Core.Extension
{
    public static class EnumerableExtensions {

        public static Random randomizer = new Random(); // you'd ideally be able to replace this with whatever makes you comfortable

        // just because the parentheses were getting confusing
        public static IEnumerable<T> GetRandom<T>(this List<T> list, int numItems) {
            
            // don't want to add the same item twice; otherwise use a list
            var items = new HashSet<T>(); 
            while (numItems > 0 )
            {
                // if we successfully added it, move on
                if( items.Add(list[randomizer.Next(list.Count)]) ) numItems--;
            }

            return items;
        }

        // and because it's really fun; note -- you may get repetition
        public static IEnumerable<T> PluckRandomly<T>(this IEnumerable<T> list) {
            while(true)
            {
                yield return list.ElementAt(randomizer.Next(list.Count()));
            }
        }
        
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                var elementValue = keySelector(element);
                if (seenKeys.Add(elementValue))
                {
                    yield return element;
                }
            }
        }

    }
}