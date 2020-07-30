using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiProgressBar : uiPanel
    {
        #region EVENTS
        public delegate void onProgressEvent(uiProgressBar c, float progress, string text);
        public event onProgressEvent onProgress;
        #endregion

        /// <summary>
        /// The current progress to be displayed stored as a floating point value from 0.0 to 1.0
        /// </summary>
        public float Value { get { return progress; } set { progress = value; text.Text = value.ToString("P0"); update_progress_area(); onProgress?.Invoke(this, progress, text.Text); } }
        protected float progress = 0f;

        public uiText text;
        public GUIStyle prog_text_style { get { return text.local_style; } set { text.local_style = value; } }

        public uiEmpty prog_bar;

        public bool show_progress_text { get { return _show_progress_text; } set { _show_progress_text = value; text.isVisible = value; } }
        private bool _show_progress_text = true;


        public uiProgressBar() : base(uiControlType.Progress)
        {
            // Create the progress bar first so it renders behind everything else.
            prog_bar = Create<uiEmpty>(this);
            prog_bar.Set_Background(Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, new Color(0.1f, 0.5f, 1.0f), new Color(1f, 1f, 1f, 1f), true, 0.3f));
            prog_bar.FloodY();

            text = Create<uiText>(this);
            text.Autosize_Method = AutosizeMethod.FILL;
            text.TextAlign = TextAnchor.MiddleCenter;
            text.disableBG = true;
            //Util.Set_BG_Color(local_style.normal, new Color(0f, 0f, 0f, 0f));

            Value = 0f;
            update_progress_area();
        }

        protected void update_progress_area() { prog_bar.Set_Width(Get_Width() * progress); }

        public override void doLayout()
        {
            update_progress_area();
            text.Set_Pos(0, 0);
            // the prog_text is set to autosize.FILL so no need to worry about flooding it
        }
    }
}
