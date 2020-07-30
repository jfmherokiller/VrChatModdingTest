using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public class Update_Method
    {
        /// <summary>
        /// The url to look for update information at.
        /// </summary>
        public string URL = null;
        /// <summary>
        /// Which updater type should be used to detect updates for the plugin.
        /// See: 
        /// </summary>
        public UPDATER_TYPE METHOD = UPDATER_TYPE.NONE;
        /// <summary>
        /// Returns <c>TRUE</c> if this instance has both a valid METHOD and a non-null non-empty URL.
        /// </summary>
        public bool Valid { get { return (METHOD != UPDATER_TYPE.NONE && !String.IsNullOrEmpty(URL)); } }


        public Update_Method(string url=null, UPDATER_TYPE method = UPDATER_TYPE.NONE)
        {
            URL = url;
            METHOD = method;
        }
    }
}
