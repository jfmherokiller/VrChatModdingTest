using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    class uiCheckbox : uiPanel
    {
        #region Variables

        public const float CHECKBOX_SIZE = 16f;
        /// <summary>
        /// The styling for the check MARK, please note that the style is only drawn when the control's isChecked property is TRUE, so the styles 'normal' state is what you wan't to set to define it's appearence.
        /// </summary>
        public GUIStyle checkmark_style = new GUIStyle();
        /// <summary>
        /// The styling for the check BOX that appears behind the check mark.
        /// </summary>
        public GUIStyle checkbox_style = new GUIStyle();
        public string text { get { return this.label.Text; } set { this.label.Text = value; update_area(); } }
        public delegate void uiCheckbox_changed_EventDelegate(uiCheckbox c, bool was_clicked);
        public event uiCheckbox_changed_EventDelegate onChange;

        private bool _checked = false;
        protected bool Checked { get { return _checked; } set { _checked = value; this.update_area(); if (onChange != null) { this.onChange(this, true); } } }
        public bool isChecked { get { return _checked; } set { _checked = value; this.update_area(); if (onChange != null) { this.onChange(this, false); } } }

        protected override bool isActive { get { return (this._checked || base.isActive); } }

        public uiText label = null;
        private Rect checkbox_area = new Rect(0, 0, CHECKBOX_SIZE, CHECKBOX_SIZE);
        #endregion


        public uiCheckbox() : base (uiControlType.Checkbox)
        {
            Clickable = true;
            Autosize = true;
            Set_Padding(1, 1, 1, 1);
            onClicked += UiCheckbox_onClicked;
            Min_Width = CHECKBOX_SIZE;
            Min_Height = CHECKBOX_SIZE;


            //border.normal.color = new Color(1f, 1f, 1f, 0.8f);
            //Utility.Set_BG_Color(checkbox_style.normal, new Color(0f, 0f, 0f, 0.5f));
            //Utility.Set_BG_Color(checkmark_style.normal, new Color(1f, 1f, 1f, 0.9f));
            checkbox_style.normal.background = TextureHelper.icon_checkbox;
            checkmark_style.normal.background = Util.Tint_Texture(TextureHelper.icon_checkmark, new Color(0.3f, 0.95f, 0.4f));

            label = Create<uiText>(this);
        }

        private void UiCheckbox_onClicked(uiControl c)
        {
            this.Checked = !this.Checked;
        }

        public override void doLayout()
        {
            label.alignLeftSide(CHECKBOX_SIZE + 3f);
        }

        protected override void Display()
        {
            base.Display();
            
            GUI.BeginGroup(inner_area);
                checkbox_style.Draw(checkbox_area, GUIContent.none, this.unity_id);
                if (this._checked) checkmark_style.Draw(checkbox_area, GUIContent.none, this.isMouseOver, this.isActive, false, false);
            GUI.EndGroup();
        }
    }
}
