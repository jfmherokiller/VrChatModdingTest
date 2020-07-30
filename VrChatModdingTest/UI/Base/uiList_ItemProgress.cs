using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// A singleline list item with a progress bar background
    /// </summary>
    class uiListItem_Progress: uiListItem
    {
        #region EVENTS
        public event Action<uiListItem_Progress, float, string> onProgress;
        #endregion

        #region Controls
        public uiEmpty prog_bar;
        public uiText prog_text;
        #endregion

        private float progress = 0f;

        #region Accessors
        public float Value { get { return progress; } set { progress = value; prog_text.Text = value.ToString("P0"); update_progress_area(); onProgress?.Invoke(this, progress, prog_text.Text); } }
        public override string Text { get { return title.Text; } set { title.Text = value; } }
        #endregion



        public uiListItem_Progress() : base(uiControlType.Progress)
        {
            title.inherits_text_style = true;
            // Create the progress bar first so it renders behind everything else.
            prog_bar = Create<uiEmpty>("progress_bar", this);
            prog_bar.Set_Background(new Color(0.1f, 0.5f, 1.0f));
            //prog_bar.Set_Background(Util.Get_Gradient_Texture(64, GRADIENT_DIR.TOP_BOTTOM, new Color(0.1f, 0.5f, 1.0f), new Color(0.2f, 0.6f, 1f, 1f), true, 0.3f));
            prog_bar.FloodY();
            this.Set_Child_Index(prog_bar, 0);// Move the progress bar to the back so it draws underneath all of the other controls here.

            prog_text = Create<uiText>(this);
            prog_text.Autosize_Method = AutosizeMethod.FILL;
            prog_text.TextAlign = TextAnchor.MiddleRight;
            prog_text.disableBG = true;
            prog_text.inherits_text_style = true;

            progress = 0f;
            update_progress_area();
        }

        protected void update_progress_area() { prog_bar.Set_Width(Area.width * progress); prog_bar.FloodY(); }

        public override void doLayout()
        {
            update_progress_area();
            title.Set_Pos(0, 0);
            prog_text.moveRightOf(title);
            // the prog_text is set to autosize.FILL so no need to worry about flooding it
        }


    }
}
