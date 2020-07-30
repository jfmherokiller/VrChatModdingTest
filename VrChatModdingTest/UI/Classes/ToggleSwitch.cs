using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SR_PluginLoader
{
    public class ToggleSwitch
    {
        /// <summary>
        /// Is the mouse overtop this control?
        /// </summary>
        private bool isHovering = false;
        /// <summary>
        /// The switch state of this control
        /// </summary>
        private bool toggled = false;
        public GUIStyle style = null, border_style = null;
        public Rect rect = new Rect();
        private const int border_size = 1;
        private const int border_size2 = (border_size*2);

        private GUIContent textContent = new GUIContent();
        private string[] text = new string[2] { "Off", "On" };


        public ToggleSwitch()
        {
            this.SetToggle(false);
        }
        
        public void SetText(string on, string off)
        {
            //we need to set the buttons text to the opposite state to the currently active one, because the text is saying what clicking the button will DO
            this.text[0] = off;
            this.text[1] = on;

            this.SetToggle(this.toggled);//update the toggle text
        }

        public void SetToggle(bool b)
        {
            this.toggled = b;
            string txt = this.text[(this.toggled ? 1 : 0)];
            this.textContent.text = txt;
        }

        public void Toggle()
        {
            this.SetToggle(!this.toggled);
        }

        public void init_skin()
        {
            this.style = new GUIStyle();
            this.style.alignment = TextAnchor.MiddleCenter;
            this.style.fontStyle = FontStyle.Bold;
            this.style.fontSize = 14;

            float g1 = 0.30f;
            float g2 = 0.15f;
            this.style.normal.background = Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, g1, g2);
            byte g = 180;
            this.style.normal.textColor = new Color32(g,g,g, 255);

            //g1 += 0.05f;
            //g2 += 0.1f;
            //this.style.hover.background = UI_Utility.Get_Gradient_Texture(64, GRADIENT_DIR.BOTTOM_TOP, g1, g2);

            g1 = 1.0f;
            g2 = 0.6f;
            this.style.active.background = Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, g1, g2, new Color32(250, 160, 0, 255));

            float blk = 0.1f;
            this.style.active.textColor = new Color(blk, blk, blk);
            blk += 0.2f;
            this.style.hover.textColor = new Color(blk, blk, blk);


            this.border_style = new GUIStyle();
            Util.Set_BG_Color(this.border_style.normal, new Color32(250, 160, 0, 180));
            Util.Set_BG_Color(this.border_style.active, new Color32(0,0,0, 255));
        }

        private void OnGUI()
        {
        }


        public bool Display()
        {
            if (this.style == null) this.init_skin();

            int id = GUIUtility.GetControlID(0, FocusType.Passive, rect);
            var evt = Event.current.GetTypeForControl(id);
            this.isHovering = rect.Contains(Event.current.mousePosition);

            switch (evt)
            {
                case EventType.MouseDown:
                    if (this.isHovering)
                    {
                        GUIUtility.hotControl = id;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        if (this.isHovering) return true;
                    }
                    return false;
                case EventType.Repaint:
                    bool focus = (GUIUtility.hotControl == id);
                    bool active = this.toggled;
                    //if (this.isDepressed) active = !active;//simulate the active state being flipped while the mouse is clicking the control.
                    
                    //this.style.Draw(rect, isHovering, true, false, focus);
                    this.border_style.Draw(new Rect(rect.position.x - border_size, rect.position.y - border_size, rect.width + border_size2, rect.height+ border_size2), GUIContent.none, isHovering || active, active, false, focus);
                    this.style.Draw(rect, this.textContent, isHovering || active, active, false, focus);
                    break;
            }

            return false;
        }




    }
}
