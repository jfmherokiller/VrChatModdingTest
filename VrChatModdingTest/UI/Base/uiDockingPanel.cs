using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// An <see cref="uiPanel"/> which completely fills all of it's parents space 
    /// </summary>
    public class uiDockingPanel: uiPanel
    {
        public uiDockingPanel() { init(); }
        public uiDockingPanel(uiControlType ty) : base(ty) { init(); }

        private void init()
        {
            Autosize = true;
            Autosize_Method = AutosizeMethod.FILL;
        }
    }
}
