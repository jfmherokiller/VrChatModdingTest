using UnityEngine;

namespace SR_PluginLoader
{
    public static class BoundsExt
    {
        public static bool HasSameSize(this Bounds a, Bounds b)
        {
            return (a.min.SameAs(b.min) && a.max.SameAs(b.max));
        }
    }
}
