using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public static class Extensions
    {
        public static string CapitalizeFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static bool Compare(this Rect A, Rect B)
        {
            return (Util.floatEq(A.x, B.x) && Util.floatEq(A.y, B.y) && Util.floatEq(A.width, B.width) && Util.floatEq(A.height, B.height));
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string ToLogString<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            var sb = new StringBuilder();
            foreach(var kvp in dict)
            {
                sb.AppendLine($"[{kvp.Key}] = {kvp.Value}");
            }

            return sb.ToString();
        }
    }
}
