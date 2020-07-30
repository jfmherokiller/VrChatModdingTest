using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public enum uiBorderType { NONE = 0, LINE, GLOW, LINE_GLOW }
    public class uiBorderStyle
    {
        public uiBorderType type = uiBorderType.LINE;
        /// <summary>
        /// Style to use by default
        /// </summary>
        public uiBorderStyleState normal = new uiBorderStyleState();
        /// <summary>
        /// Style to use when the mouse is overtop the control
        /// </summary>
        public uiBorderStyleState hover = new uiBorderStyleState();
        /// <summary>
        /// Style to use when the control is in an 'activated' state
        /// </summary>
        public uiBorderStyleState active = new uiBorderStyleState();
        /// <summary>
        /// Style to use when the control has input focus.
        /// </summary>
        public uiBorderStyleState focused = new uiBorderStyleState();

        /// <summary>
        /// Used to store the final values as the seperate fields of each stylestate all cascade downwards and override each others values for each applicable state to the control to reach the final values that should be used.
        /// </summary>
        public uiBorderStyleState _cached = new uiBorderStyleState();
    }
}
