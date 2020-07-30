using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    /// <summary>
    /// Multiline version of the uiText component.
    /// </summary>
    public class uiTextArea : uiText
    {
        public uiTextArea() : base(uiControlType.TextArea) { init(); }
        public uiTextArea(uiControlType type) : base(type) { init(); }

        private void init()
        {
        }
        /*
        /// <summary>
        /// Overriden to ensure the text doesn't exceed the parent controls bounds
        /// </summary>
        protected override Vector2 Get_Autosize(Vector2? starting_size = null)
        {
            if (content == null || content.text == null || content.text.Length <= 0) return new Vector2(0, 0);
            //make sure text will not run outside of it's parent controls bounds.
            Vector2 sz = base.Get_Autosize(starting_size);
            if (isChild)
            {
                float pw = (parent.Get_Content_Area().width - area.x);
                if (pw > 0f)
                {
                    sz.x = Mathf.Min(sz.x, pw);
                    sz.y = styleText.CalcHeight(content, sz.x);
                }
            }

            return sz;
        }
        */
    }
}
