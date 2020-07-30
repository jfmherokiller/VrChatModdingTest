
using UnityEngine;

namespace SR_PluginLoader
{
    public static class VectorExt
    {
        /// <summary>
        /// Returns a new vector with the same values as the given one.
        /// </summary>
        public static Vector2 Clone(this Vector2 v) { return new Vector2(v.x, v.y); }
        /// <summary>
        /// Returns a new vector with the same values as the given one.
        /// </summary>
        public static Vector3 Clone(this Vector3 v) { return new Vector3(v.x, v.y, v.z); }

        /// <summary>
        /// Returns TRUE if X or Y is a nonzero value
        /// </summary>
        public static bool NonZero(this Vector2 v) { return (!Util.floatEq(0f, v.x) || !Util.floatEq(0f, v.y)); }
        /// <summary>
        /// Returns TRUE if X or Y is a nonzero value
        /// </summary>
        public static bool NoZero(this Vector2 v) { return (!Util.floatEq(0f, v.x) && !Util.floatEq(0f, v.y)); }

        /// <summary>
        /// Returns TRUE if X, Y, or Z is a nonzero value
        /// </summary>
        public static bool NonZero(this Vector3 v) { return (!Util.floatEq(0f, v.x) || !Util.floatEq(0f, v.y) || !Util.floatEq(0f, v.z)); }
        /// <summary>
        /// Returns TRUE if X, Y, or Z is a nonzero value
        /// </summary>
        public static bool NoZero(this Vector3 v) { return (!Util.floatEq(0f, v.x) && !Util.floatEq(0f, v.y) && !Util.floatEq(0f, v.z)); }

        /// <summary>
        /// Returns <c>True</c> if a given vector has the same X/Y/Z values.
        /// </summary>
        public static bool SameAs(this Vector3 v, Vector3 b) { return (Util.floatEq(v.x, b.x) && Util.floatEq(v.y, b.y) && Util.floatEq(v.z, b.z)); }

        /// <summary>
        /// Returns <c>True</c> if a given vector extends less than this vector on the X, Y, or Z axis.
        /// </summary>
        public static bool LessThan(this Vector3 v, Vector3 b) { return (v.x < b.x || v.y < b.y || v.z < b.z); }

        /// <summary>
        /// Returns <c>True</c> if a given vector extends further than this vector on the X, Y, or Z axis.
        /// </summary>
        public static bool GreaterThan(this Vector3 v, Vector3 b) { return (v.x > b.x || v.y > b.y || v.z > b.z); }



        public static Vector3 Min(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        public static Vector3 Max(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }
    }
}
