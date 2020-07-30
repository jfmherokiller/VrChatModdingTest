using System;
using System.Collections.Generic;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiWindow : uiPanel
    {

        protected override List<uiControl> children { get { if (content_panel==null) { throw new Exception("uiWindow content_panel not ready!"); } return content_panel.Get_Children(); } }
        /// <summary>
        /// Tracks all uiWindow instances by their ID.
        /// </summary>
        private static new List<int> ALL = new List<int>();
        private static Texture2D title_bar_texture = null;

        #region Components

        protected uiScrollPanel content_panel = null;
        private GUIStyle style_title = null, style_titlebar = null;
        private int title_bar_height { get { return 26; } }
        private bool dragging = false;
        private uiButton closeBtn = null;
        #endregion

        #region Events

        public event controlEvent<uiWindow> onClosed;
        public event controlEvent<uiWindow> onShown;
        public event controlEvent<uiWindow> onHidden;
        #endregion

        #region Variables

        /// <summary>
        /// Specifies weather or not this window can be moved around by the player.
        /// </summary>
        public bool draggable = true;
        private Rect title_area = new Rect(), titlebar_area = new Rect(), title_stipple_area = new Rect(), title_stipple_coords = new Rect(), titlebar_buttons_area = new Rect();
        private Rect titlebar_areaAbs { get { return relToAbsolute(titlebar_area); } }
        public override Rect content_area { get { return content_panel.content_area; } }
        protected override Rect inner_area { get { return content_panel.Get_Content_Area(); } }
        public string Title { get { return content.text; } set { content.text = String.Format("<b>{0}</b>", value); dirty_layout = true; } }
        #endregion

        // Size defines for the stippled titlebar pattern
        const int stipple_pattern_w = 4;
        const int stipple_pattern_h = 5;


        #region Constructors

        public uiWindow() : base(uiControlType.Window)
        {
            ALL.Add(ID);

            isDraggable = true;// among other things; this control cannot have it's position assigned via auto-positioners.
            Autosize = false;
            isVisible = false;//hidden by default
            Title = "Window";
            //selfPadding = new RectOffset(0, 0, title_bar_height, 0);
            //local_style.fontStyle = FontStyle.Bold;
            //local_style.fontSize = 32;

            Setup_Titlebar();


            closeBtn = Create<uiButton>();
            GUIStyle sty = new GUIStyle();
            sty.normal.background = TextureHelper.icon_close_dark;
            sty.hover.background = TextureHelper.icon_close;
            closeBtn.Border.hover.size = new RectOffset(1, 1, 1, 1);
            closeBtn.Border.hover.color = new Color(1f, 1f, 1f, 0.2f);
            closeBtn.Border.type = uiBorderType.NONE;//we don't ever want the close button to render a border.

            closeBtn.Set_Size(title_bar_height, title_bar_height);
            closeBtn.Set_Style(sty);
            closeBtn.onClicked += CloseBtn_onClicked;
            closeBtn.Set_Margin(6);


            content_panel = Create<uiScrollPanel>();
            content_panel.Set_Margin(5, 5, 0, 5);
            content_panel.Autosize = true;
            content_panel.Autosize_Method = AutosizeMethod.FILL;
            content_panel.Border.normal.color = new Color(0f, 0f, 0f, 0.35f);// A semi-transparent black to darken the edges around the content area so it's more distinguishable.

            this.Add_Control(content_panel);
            this.Add_Control(closeBtn);

            this.layout_content_area();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ALL.Remove(ID);
        }
        #endregion

        #region Overrides

        public override string ToString() { return String.Format("[{0}] {1}", Title, base.ToString()); }
        #endregion

        #region Class Events

        private void Setup_Titlebar()
        {
            style_titlebar = new GUIStyle();
            Util.Set_BG_Color(style_titlebar.normal, new Color(1f, 1f, 1f, 0.05f));

            style_title = new GUIStyle();
            style_title.normal.textColor = Color.white;
            style_title.fontSize = 14;
            style_title.alignment = TextAnchor.MiddleLeft;
            style_title.clipping = TextClipping.Clip;
            //style_title.fontSize = 32;

            if (title_bar_texture == null)
            {
                var tex = new Texture2D(stipple_pattern_w, title_bar_height);
                tex.wrapMode = TextureWrapMode.Repeat;
                Color clearClr = new Color(0f, 0f, 0f, 0f);

                // Clear the textures pixels so they are all transparent
                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        tex.SetPixel(x, y, clearClr);
                    }
                }

                Color pClr = new Color(1f, 1f, 1f, 0.3f);
                // Now create the stippled pattern we want
                int cY = (int)(((float)tex.height / 2f) - ((float)stipple_pattern_h / 2f) + 1);
                tex.SetPixel(0, cY - 2, pClr);
                tex.SetPixel(2, cY, pClr);
                tex.SetPixel(0, cY + 2, pClr);

                tex.Apply();
                title_bar_texture = tex;
            }
        }
        
        private void CloseBtn_onClicked(uiControl c) { this.Close(); }
        #endregion

        #region uiWindow-Specific functions

        /// <summary>
        /// Shows a specified window while hiding all others.
        /// </summary>
        /// <param name="window">The window to switch to.</param>
        public static void Switch(uiWindow window)
        {
            try
            {
                foreach (int wid in uiWindow.ALL) { if (wid != window.ID) (uiControl.ALL[wid] as uiWindow).Hide(); }
            }
            finally
            {
                window.Show();
            }
        }

        public void Close()
        {
            Hide();
            onClosed?.Invoke(this);// fire event
        }

        public void Show()
        {
            if (isVisible) return;
            isVisible = true;

            if (Game.atMainMenu)
                MainMenu.Hide();
            else
                GameTime.Pause();

            onShown?.Invoke(this);// fire event
        }

        public void Hide()
        {
            if (!isVisible) return;
            isVisible = false;

            if (Game.atMainMenu)
                MainMenu.Show();
            else
                GameTime.Unpause();

            onHidden?.Invoke(this);// fire event
        }

        public void ToggleShow()
        {
            if (!isVisible) Show();
            else Hide();
        }

        public void Center()
        {
            float X = (Screen.width * 0.5f) - (Area.width * 0.5f);
            float Y = (Screen.height * 0.5f) - (Area.height * 0.5f);

            this.Set_Pos(new Vector2(X, Y));
        }
        #endregion

        #region Remap Add / Remove / Clear functions to our content_panel control

        public override uiControl Add(uiControl c) { return content_panel.Add(c); }

        public override uiControl Add(string name, uiControl c) { return content_panel.Add(name, c); }

        public override bool Remove(uiControl c) { return content_panel.Remove(c); }

        public override bool Remove(string name) { return content_panel.Remove(name); }

        public override void Clear_Children() { content_panel.Clear_Children(); }

        public override bool withinChild(Vector2 p)
        {
            if (closeBtn.Area.Contains(p)) return true;
            return content_panel.withinChild(p);
        }

        //public override IList<uiControl> Get_Children() { return content_panel.Get_Children(); }
        #endregion

        public override void doLayout()
        {
            titlebar_area = new Rect(0, 0, Area.width, title_bar_height);

            int btn_area_width = (title_bar_height + 4);
            titlebar_buttons_area = new Rect(titlebar_area.width-btn_area_width, 0, btn_area_width, titlebar_area.height);

            Vector2 tsz = style_title.CalcSize(content);
            title_area = new Rect(6f, 0f, tsz.x+8f, titlebar_area.height);

            //ensure the stipple pattern always ends on the first column of the next repetition
            int stipple_width = (int)(titlebar_buttons_area.xMin - title_area.xMax - 6f);
            if ((stipple_width % stipple_pattern_w) != 0) stipple_width = (((stipple_width / stipple_pattern_w) * stipple_pattern_w) + 0);// Make sure the drawn texture always ends on the last pixel no matter how many times it repeats

            title_stipple_area = new Rect(title_area.xMax, 0f, stipple_width, titlebar_area.height);
            title_stipple_coords = new Rect(0f, 0f, (title_stipple_area.width/(float)stipple_pattern_w), 1f);
            
            closeBtn.Set_Pos(titlebar_area.width - title_bar_height, 0);
            //closeBtn.area = new Rect(titlebar_area.xMax - close_btn_size, titlebar_area.yMin, close_btn_size, close_btn_size);
            layout_content_area();
            base.doLayout();
        }

        private void layout_content_area()
        {
            float content_vs_title_padding = 2f;
            content_panel.Set_Pos(0, title_bar_height);
            
            content_panel.Margin.top = (int)content_vs_title_padding;
            content_panel.Set_Size(_inner_area.width, (_inner_area.height - (content_vs_title_padding-1) - title_bar_height));
        }

        public override void handleEvent()
        {
            if (!isVisible || isDisabled)
            {
                SLog.Info("[{0}] Window handling events while INVISIBLE!", this);
                return;
            }

            Event evt = Event.current;
            bool use_event = false;

            switch (evt.GetTypeForControl(unity_id))
            {
                case EventType.MouseDrag:
                    if(dragging)
                    {
                        use_event = true;
                        Vector2 dtPos = evt.delta;
                        Set_Pos(dtPos.x+Area.x, dtPos.y+Area.y);
                    }
                    break;
                case EventType.MouseDown:
                    dragging = false;
                    if (titlebar_areaAbs.Contains(evt.mousePosition) && draggable) dragging = true;
                    break;
                case EventType.MouseUp:
                    dragging = false;
                    break;
            }

            base.handleEvent();
            if (use_event && Event.current.GetTypeForControl(unity_id) != EventType.Used)
            {
                evt.Use();
            }
        }

        protected override void Display()
        {
            Display_BG();// Draw BG
            //GUI.BeginGroup(title_bar_abs_area);
            GUI.BeginGroup(draw_area);
            GUI.BeginGroup(titlebar_area);
                    style_title.Draw(title_area, content, false, false, false, false);// Draw the titlebar text.
                    GUI.DrawTextureWithTexCoords(title_stipple_area, title_bar_texture, title_stipple_coords, true);
                GUI.EndGroup();
            GUI.EndGroup();

            //GUI.BeginClip(_inner_area);
            GUI.BeginGroup(_inner_area);

            for (int i = 0; i < children.Count; i++)
            {
                if(CONFIRM_DRAW) SLog.Info("  - Drawing child: {0}", children[i]);
                children[i].TryDisplay();
            }

            GUI.EndGroup();
            //GUI.EndClip();
        }

        private void Update()
        {
            if (!isVisible) return;
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Close();
            }
        }
    }
}
