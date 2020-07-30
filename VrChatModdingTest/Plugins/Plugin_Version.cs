using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SR_PluginLoader
{
    public class Plugin_Version
    {
        public int Major;
        public int Minor;
        public int Build;

        public Plugin_Version(int _major, int _minor = 0)
        {
            Major = _major;
            Minor = _minor;
            Build = ComputeBuildNo(Assembly.GetCallingAssembly());
        }
        
        public Plugin_Version(int _major, int _minor, int _patch)
        {
            Major = _major;
            Minor = _minor;
            Build = _patch;
        }

        private int ComputeBuildNo(Assembly asy)
        {
            Version v = asy.GetName().Version;
            return ((v.Build << 16) + v.Revision);
        }

        public override string ToString()
        {
            return String.Format("v{0}.{1}.{2}", Major, Minor, Build);
        }

        public static bool operator ==(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) == 0;
        }

        public static bool operator !=(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) != 0;
        }

        public static bool operator <(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) < 0;
        }

        public static bool operator >(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) > 0;
        }

        public static bool operator <=(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) <= 0;
        }

        public static bool operator >=(Plugin_Version v1, Plugin_Version v2)
        {
            return v1.Compare(v2) >= 0;
        }

        protected long toInt()
        {
            return ((Major << 32) + (Minor << 16) + Build);
        }

        public int Compare(Plugin_Version other)
        {
            return (int)Math.Max(-1, Math.Min(1, this.toInt() - other.toInt()));
        }

        public override int GetHashCode()
        {
            return (int)toInt();
        }

        public override bool Equals(object obj)
        {
            Plugin_Version vers = obj as Plugin_Version;
            return 0==Compare(vers);
        }
    }
}
