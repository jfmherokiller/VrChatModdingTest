using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    /// <summary>
    /// A uiPanel with scrolling enabled by default, for semantics.
    /// </summary>
    public class uiScrollPanel : uiPanel
    {
        // Scroll panels come with a darkened background by default (it's for the best)
        public uiScrollPanel() : base(uiControlType.Panel_Dark) { Scrollable = true; }
        public uiScrollPanel(uiControlType type) : base(type) { Scrollable = true; }
    }
}
