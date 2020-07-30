using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public enum AutosizeMethod
    {
        /// <summary>
        /// Maintain a set size.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Allow size growth if child controls exceed the current bounds
        /// Any user specified size is treated as the MINIMUM size instead of the constant one.
        /// </summary>
        GROW,
        /// <summary>
        /// Allow the control to shrink so it only takes up as much space as it needs.
        /// The User-Specified size is treated as the MAXIMUM size instead of the constant one.
        /// </summary>
        SHRINK,
        /// <summary>
        /// The control will act like an HTML block-level element and flood its parent on the x axis.
        /// </summary>
        BLOCK,
        /// <summary>
        /// The control will fill the remaining space within it's parent.
        /// </summary>
        FILL,
        /// <summary>
        /// The control will fill the remaining space within it's parent on the Y axis, then set it's width to match.
        /// </summary>
        ICON_FILL,
    }
}
