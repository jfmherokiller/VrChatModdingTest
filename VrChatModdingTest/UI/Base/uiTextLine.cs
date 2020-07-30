using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiTextLine : uiText
    {
        public uiTextLine()
        {
            Autosize_Method = AutosizeMethod.BLOCK;
            Autosize = true;
            Set_Padding(3, 2);
            Set_Background(new Color(0.1f, 0.1f, 0.1f, 0.2f));
        }
    }
}
