using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Provides extension functions for Unity's Color class. 
    /// </summary>
    public static class ColorExt
    {
        /// <summary>
        /// Sets the Alpha component for a color instance and returns the instance.
        /// </summary>
        public static Color SetAlpha(this Color c, float a)
        {
            c.a = a;
            return c;
        }

        /// <summary>
        /// Adds an ammount to the RGB components of a <see cref="UnityEngine.Color"/> object
        /// </summary>
        public static Color Add(this Color clr, float v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r += v;
            c.g += v;
            c.b += v;
            return c;
        }

        /// <summary>
        /// Adds an ammount to the RGB components of a <see cref="UnityEngine.Color"/> object
        /// </summary>
        public static Color Add(this Color clr, double v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r += (float)v;
            c.g += (float)v;
            c.b += (float)v;
            return c;
        }



        /// <summary>
        /// Subtracts an ammount to the RGB components of a <see cref="UnityEngine.Color"/> object
        /// </summary>
        public static Color Sub(this Color clr, float v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r -= v;
            c.g -= v;
            c.b -= v;
            return c;
        }

        /// <summary>
        /// Subtracts an ammount to the RGB components of a <see cref="UnityEngine.Color"/> object
        /// </summary>
        public static Color Sub(this Color clr, double v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r -= (float)v;
            c.g -= (float)v;
            c.b -= (float)v;
            return c;
        }



        /// <summary>
        /// Multiplies the RGB components of a <see cref="UnityEngine.Color"/> object by a given value
        /// </summary>
        public static Color Mul(this Color clr, float v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r *= v;
            c.g *= v;
            c.b *= v;
            return c;
        }

        /// <summary>
        /// Multiplies the RGB components of a <see cref="UnityEngine.Color"/> object by a given value
        /// </summary>
        public static Color Mul(this Color clr, double v)
        {
            Color c = new Color(clr.r, clr.g, clr.b, clr.a);
            c.r *= (float)v;
            c.g *= (float)v;
            c.b *= (float)v;
            return c;
        }
    }
}
