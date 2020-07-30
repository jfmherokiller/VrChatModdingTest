using System;
using Harmony;
using MelonLoader;
using UnityEngine;
using String = Il2CppSystem.String;

namespace ObjLoader.Loader.Common
{
    public static class StringExtensions
    {
        public static AssetBundle[] AddToArray(AssetBundle[] target, AssetBundle item)
        {
            if (target == null)
            {
                //TODO: Return null or throw ArgumentNullException;
            }
            var result = new AssetBundle[target.Length + 1];
            target.CopyTo(result, 0);
            result[target.Length] = item;
            return result;
        }
        public static T[] AddToArray2<T>(this T[] target, T item)
        {
            if (target == null)
            {
                //TODO: Return null or throw ArgumentNullException;
            }
            T[] result = new T[target.Length + 1];
            target.CopyTo(result, 0);
            result[target.Length] = item;
            return result;
        }
        public static String[][] AddToArraySpecial(String[][] target, String[] item)
        {
            if (target == null)
            {
                //TODO: Return null or throw ArgumentNullException;
            }

            var result = (String[][])target.Clone();
            result.AddToArray(item);
            return result;
        }
        public static float ParseInvariantFloat(this string floatString)
        {
            float myfloat;
            if(!float.TryParse(floatString,out myfloat))
            {
                myfloat = 1.0f;
            }
            return myfloat;
        }

        public static int ParseInvariantInt(this string intString)
        {
            int mystring;
            if(!int.TryParse(intString, out mystring))
            {
                mystring = 1;
            }


            MelonModLogger.Log(intString);
            return mystring;
        }

        public static bool EqualsInvariantCultureIgnoreCase(this string str, string s)
        {
            return str.Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrEmpty(str.Trim());
        }
    }
}