using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    /// <summary>
    /// A two part text component which allows us to easily display a changing variable text value right next to an unchanging label text value which describes it.
    /// </summary>
    public class uiVarText : uiPanel
    {
        private uiText _text = null, _value = null;
        /// <summary>
        /// The unchanging text to print at the left of the value
        /// </summary>
        public override string Text { get { return _text.Text; } set { _text.Text = String.Format("<b>{0}</b> ", value); update_area(); } }
        /// <summary>
        /// The value string to print at the right of the text
        /// </summary>
        public string Value { get { return _value.Text; } set { _value.Text = value; update_area(); } }

        public GUIStyle text_style { get { return _text.local_style; } set { _text.local_style = value; } }
        public GUIStyle value_style { get { return _value.local_style; } set { _value.local_style = value; } }

        public uiVarText() : base(uiControlType.Text)
        {
            Autosize_Method = AutosizeMethod.GROW;
            _text = Create<uiText>(this);
            _value = Create<uiText>(this);
        }

        public override void doLayout()
        {
            _text.Set_Pos(0, 0);
            _value.sitRightOf(_text);
        }

    }
}
