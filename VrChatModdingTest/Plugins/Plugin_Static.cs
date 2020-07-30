using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public partial class Plugin
    {
        public static Plugin Get_Plugin(string name_hash)
        {
            foreach (KeyValuePair<string, Plugin> kv in Loader.plugins)
            {
                if (String.Compare(kv.Value.Hash, name_hash) == 0) return kv.Value;
            }

            return null;
        }

        public static bool Is_Plugin_Installed(Plugin_Data data)
        {
            return Is_Plugin_Installed(data.Hash);
        }

        public static bool Is_Plugin_Installed(string hash)
        {
            foreach (KeyValuePair<string, Plugin> kv in Loader.plugins)
            {
                if (kv.Value == null || kv.Value.data == null) { SLog.Error("@ Loader.Is_Plugin_Installed(): Value for KeyValuePair @ Key {0} is not a valid/loaded plugin!", kv.Key); }
                else if (String.Compare(kv.Value.Hash, hash) == 0) return true;
            }
            return false;
        }
    }
}
