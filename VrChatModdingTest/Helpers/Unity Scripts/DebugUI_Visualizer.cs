using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class DebugUI_Visualizer : MonoBehaviour
    {
        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            //GL.PushMatrix();

            if (MaterialHelper.mat_line.SetPass(0))
            {
                //DebugUI.DEBUG_LINE_MAT.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Color(uiControl.color_purple);
                DebugUI.Draw_GameObj_Bounds(base.gameObject);
                GL.End();
            }
        }
    }
}
