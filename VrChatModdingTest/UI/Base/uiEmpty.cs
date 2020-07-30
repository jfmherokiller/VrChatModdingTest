using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// It's just an empty box. Useful for having a solid colored area or whatnot. EG: progress bar background.
    /// </summary>
    public class uiEmpty : uiControl
    {
        public uiEmpty() { }

        protected override void Display()
        {
            Display_BG();
        }
    }
}
