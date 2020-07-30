using System;
using System.Collections.Generic;
using System.Linq;

namespace SR_PluginLoader
{
    public static class TypeExt
    {
        public static bool IsOneOf(this Type ty, params Type[] types) { return types.Contains(ty); }
    }
}
