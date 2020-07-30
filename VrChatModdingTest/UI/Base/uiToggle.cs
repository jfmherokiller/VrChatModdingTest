using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    /// <summary>
    /// A button that can be visually toggled on or off.
    /// </summary>
    public class uiToggle : uiButton
    {
        #region Variables

        private string[] text = new string[2] { "Enable", "Disable" };

        /// <summary>
        /// The text to display when toggled off
        /// </summary>
        public string text_off { get { return text[0]; } set { text[0] = value; update_area(); } }

        /// <summary>
        /// The text to display when toggled on
        /// </summary>
        public string text_on { get { return text[1]; } set { text[1] = value; update_area(); } }

        private bool _checked = false;
        protected bool Checked { get { return _checked; } set { _checked = value; onChange?.Invoke(this, true); update_text(); } }
        public bool isChecked { get { return _checked; } set { _checked = value; onChange?.Invoke(this, false); update_text(); } }

        protected override bool isActive { get { return (_checked || base.isActive); } }

        //public delegate void uiToggle_changed_EventDelegate<T>(T c, bool was_clicked) where T : uiControl;
        public event Action<uiToggle, bool> onChange;

        private Color[] text_clr = new Color[2] { new Color(0.7f, 0.7f, 0.7f), new Color32(16, 16, 16, 255) };
        #endregion

        public uiToggle() : base()
        {
            Clickable = true;
            onClicked += UiToggle_onClicked;

            //set the default text styling
            TextAlign = TextAnchor.MiddleCenter;

            //this.local_style.hover.textColor = new Color(1f, 1f, 1f);// text color for when the BG is black and the mouse is over

            // borders
            Border.hover.reset();
            Border.active.color = new Color32(0, 0, 0, 255);// black
            Border.normal.color = new Color32(250, 160, 0, 180);// orange

            // background designs
            local_style.hover.background  = null;
            local_style.normal.background = Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, 0.3f, 0.15f);// black
            local_style.active.background = Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, 1.0f, 0.6f, new Color32(250, 160, 0, 255));// orange
        }
        
        private void UiToggle_onClicked(uiControl c) { Checked = !Checked; }

        private void update_text(bool do_area_update = true)
        {
            content.text = (_checked ? this.text_on : this.text_off);
            local_style.normal.textColor = (_checked ? this.text_clr[1] : this.text_clr[0]);
            if (do_area_update) update_area();
        }

        public override void doLayout()
        {
            update_text(false);
        }

        /*
        protected override void Display()
        {
            Display_BG();// Draw Background
            styleText.Draw(draw_area, content, this.isMouseOver || this.isActive, this.isActive, false, this.isFocused);//Draw text
        }
        */

    }
}
