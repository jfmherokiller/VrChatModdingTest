using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public enum PLUGIN_DEP_COMPARISON_FLAG
    {
        FALSE = 0,
        SAME,
        VERSION_TOO_LOW,
        VERSION_TOO_HIGH
    }

    public class Plugin_Dependency
    {
        /// <summary>
        /// The DLL file name for the specified plugin
        /// </summary>
        public string NAME;
        /// <summary>
        /// The minimum required version of the specified plugin, or NULL if none
        /// </summary>
        public Plugin_Version MIN_VERSION = null;
        
        public Plugin_Dependency(string dll_name)
        {
            this.NAME = dll_name;
        }


        public PLUGIN_DEP_COMPARISON_FLAG Compare(Plugin obj)
        {
            if (this.NAME != obj.data.NAME) return PLUGIN_DEP_COMPARISON_FLAG.FALSE;

            int v = this.MIN_VERSION.Compare(obj.data.VERSION);
            if (v < 0) return PLUGIN_DEP_COMPARISON_FLAG.VERSION_TOO_LOW;

            return PLUGIN_DEP_COMPARISON_FLAG.SAME;
        }
    }
}
