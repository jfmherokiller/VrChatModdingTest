using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// Panel that sizes to it's contents by default.
    /// </summary>
    public class uiWrapperPanel : uiPanel
    {
        public uiWrapperPanel() : base() { init(); }
        public uiWrapperPanel(uiControlType type) : base(type) { init(); }

        private void init()
        {
            Autosize_Method = AutosizeMethod.GROW;
            Autosize = true;//auto size by default until the user gives us an explicit size
        }
    }
}
