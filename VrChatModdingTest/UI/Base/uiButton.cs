using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiButton : uiControl
    {
        /// <summary>
        /// Should this control play a sound when clicked?
        /// </summary>
        public bool playClickSound = true;


        public uiButton() : base(uiControlType.Button)
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
        }

        private void Clicked_Snd_Handler(uiControl c)
        {
            if (playClickSound) Sound.Play(SoundId.BTN_CLICK);
        }
        
        protected override void Display()
        {
            Display_BG();// Draw BG
            Display_Text();// Draw our text
        }
                
    }
}
