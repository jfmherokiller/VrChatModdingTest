
using UnityEngine;

namespace SR_PluginLoader
{


    public class uiIconButton : uiPanel
    {
        /// <summary>
        /// Should this control play a sound when clicked?
        /// </summary>
        public bool playClickSound = true;

        private uiText text = null;
        private uiIcon icon = null;

        public Texture2D Icon { get { return icon.Image; } set { icon.Image = value; icon.Autosize_Method = (value==null ? AutosizeMethod.NONE : AutosizeMethod.ICON_FILL); set_layout_dirty(); } }// We don't update the area after this because if we just did that then the text would never shift over to make room or anything.
        public override string Text { get { return text.Text; } set { text.Text = value; set_area_dirty(); } }


        public uiIconButton() : base(uiControlType.Button)
        {
            Clickable = true;
            Autosize = true;
            Autosize_Method = AutosizeMethod.GROW;
            Set_Padding(2, 2, 1, 1);

            Border.normal.color = new Color(1f, 1f, 1f, 0.3f);
            Border.hover.color = new Color(1f, 1f, 1f, 0.8f);
            Border.normal.stipple_size = 2;
            Border.normal.stipple_gap = 1;

            onClicked += Clicked_Snd_Handler;

            icon = Create<uiIcon>("icon", this);
            icon.disableBG = true;

            text = Create<uiText>("text", this);
            text.inherits_text_style = true;
            text.Autosize = true;
            text.Autosize_Method = AutosizeMethod.GROW;
        }

        private void Clicked_Snd_Handler(uiControl c)
        {
            if (playClickSound) Sound.Play(SoundId.BTN_CLICK);
        }

        public override void doLayout()
        {
            base.doLayout();
            icon.Set_Pos(0, 0);
            //text.moveRightOf(icon, icon.Get_Width() > 0 ? 4 : 0);
            text.moveRightOf(icon, 0);
        }
    }
}
