using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public enum DialogResult
    {
        NONE=0,
        OK=1,
        CANCEL=2,
    }
    /// <summary>
    /// Provides the basis for a popup dialogue box that can be used to prompt the user for a response to given data in the form of controls added to the dialogues "contentPanel" control.
    /// </summary>
    public class uiDialogResult : uiWindow
    {
        #region Controls

        protected uiTextArea message = null;
        protected uiScrollPanel contentPanel = null;
        protected uiButton btn_ok = null, btn_later = null;
        protected uiWrapperPanel btnPanel = null;
        #endregion

        public override string Text { get { return message.Text; } set { message.Text = value; } }

        #region Results

        private DialogResult result = DialogResult.NONE;
        public DialogResult Result { get { return result; }
            set
            {
                result = value;
                if(onResult==null) Hide();
                else
                {
                    bool b = onResult.Invoke(result);
                    if (!b) Hide();
                }
            }
        }

        /// <summary>
        /// A callback to fire which handles user input for the dialog control.
        /// Return <c>TRUE</c> to leave the dialog control visible.
        /// </summary>
        public DialogResultDelegate onResult;
        public delegate bool DialogResultDelegate(DialogResult result);
        #endregion


        public uiDialogResult()
        {
            Set_Size(450, 300);
            Center();

            message = Create<uiTextArea>("msg", this);
            message.Autosize_Method = AutosizeMethod.BLOCK;
            message.Autosize = true;
            message.Text = "Lorum Ipsum Lorum Ipsum\nLorum Ipsum Lorum Ipsum";
            message.TextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

            contentPanel = Create<uiScrollPanel>("content", this);
            contentPanel.Set_Margin(0, 6);

            btnPanel = Create<uiWrapperPanel>("btn_panel", this);
            btnPanel.Autosize_Method = AutosizeMethod.BLOCK;
            btnPanel.Autosize = true;
            btnPanel.onLayout += BtnPanel_onLayout;

            btn_ok = Create<uiButton>("btn_ok", btnPanel);
            btn_ok.Text = "OK";
            btn_ok.onClicked += (uiControl c) => { Result = DialogResult.OK; };
            btn_ok.Set_Padding(5, 2);

            btn_later = Create<uiButton>("btn_later", btnPanel);
            btn_later.Text = "Later";
            btn_later.onClicked += (uiControl c) => { Result = DialogResult.CANCEL; };
            btn_later.Set_Padding(5, 2);
        }

        private void BtnPanel_onLayout(uiPanel c)
        {
            btn_ok.alignLeftSide();
            btn_later.alignRightSide();
        }

        public override void doLayout()
        {
            base.doLayout();

            message.Set_Pos(0, 0);
            btnPanel.alignBottom();

            contentPanel.moveBelow(message);
            contentPanel.snapBottomSideTo(btnPanel);
            contentPanel.FloodX();
        }

        public void Enable_Buttons()
        {
            btn_ok.isDisabled = false;
            btn_later.isDisabled = false;
        }

        public void Disable_Buttons()
        {
            btn_ok.isDisabled = true;
            btn_later.isDisabled = true;
        }
    }
}
