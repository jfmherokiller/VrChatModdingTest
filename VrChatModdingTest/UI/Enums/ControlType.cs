using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public enum uiControlType
    {
        Generic = 0,
        Panel,
        Panel_Dark,
        Window,
        Button,
        Text,
        Textbox,
        /// <summary>
        /// A read-only multiline textbox
        /// </summary>
        TextArea,
        Checkbox,
        Progress,
        Icon,
        ListItem,
    }

}
