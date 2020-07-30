using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SR_PluginLoader
{
    public class DebugHUD_Renderer : MonoBehaviour
    {
        private const float PANEL_WIDTH = 500f;
        private List<string> lines = new List<string>();
        private Dictionary<string, int> stacks = new Dictionary<string, int>();
        //private String lines_joined = "";
        private GUIContent console_lines = new GUIContent();
        private GUIContent alert_content = new GUIContent();
        private GUIContent alert_sub_content = new GUIContent();
        private GUIContent watermark_content = new GUIContent();
        private GUIContent watermark_text_content = new GUIContent();
        private GUIContent player_pos_text = new GUIContent();
        
        private bool needs_layout = true;

        private Rect screen_area, console_area, console_inner_area, console_inner_text_area, fade_area, watermark_text_area, watermark_area, player_pos_area;

        private const float alert_size = 32f;
        private const float alert_icon_offset = 10f;
        private Rect alertPos = new Rect(alert_icon_offset, alert_icon_offset, alert_size, alert_size);
        private Rect alert_txtPos = new Rect(0f, 0f, 0f, 0f);
        private Rect alert_sub_txtPos = new Rect(0f, 0f, 0f, 0f);
        private Vector2 alert_txtSz = new Vector2();
        private Vector2 alert_sub_txtSz = new Vector2();
        private Vector2 console_scroll = Vector2.zero;

        private static bool dirty_styles = true;// do the styles need to be updated?

        private static Texture2D bg_fade = null;
        private static GUIStyle blackout = new GUIStyle();
        private static GUIStyle text_style = new GUIStyle();
        private static GUIStyle subtext_style = new GUIStyle();
        private static GUIStyle console_text_style = new GUIStyle();
        private static GUIStyle watermark_style = new GUIStyle();



        private const float scrollbar_width = 6f;

        private static GUISkin skin = null;

        private bool open = false;
        private int new_count = 0;
        private int id = 0;

        private KeyCode OPEN_KEY = KeyCode.Tab;


        public void Awake()
        {
            this.Clear();
        }

        public void Clear()
        {
            this.lines.Clear();
            this.stacks.Clear();
        }

        public void Add_Line(string str)
        {
            this.new_count++;
            this.lines.Add(str);

            this.console_lines.text = String.Join("\n", this.lines.ToArray());
            string msg = String.Format("{0} new log{1}", this.new_count, this.new_count>1 ? "s" : "");
            alert_content.text = msg;
            alert_sub_content.text = String.Format("<i>Press <b>{0}</b> to open the plugins console.</i>", this.OPEN_KEY);
            this.needs_layout = true;
        }

        public void Add_Tally(string str, int cnt=1)
        {
            int tmp = 0;
            if (!this.stacks.TryGetValue(str, out tmp)) this.stacks[str] = 0;

            this.stacks[str] += cnt;
        }

        private void Init_Styles()
        {
            dirty_styles = false;
            DebugHUD_Renderer.bg_fade = Util.Get_Gradient_Texture(400, GRADIENT_DIR.LEFT_RIGHT, new Color(0f,0f,0f,1f), new Color(0f,0f,0f,0f), true, 1.5f);
            Util.Set_BG_Color(DebugHUD_Renderer.blackout.normal, 0.1f, 0.1f, 0.1f, 0.7f);

            DebugHUD_Renderer.text_style.normal.textColor = Color.white;
            DebugHUD_Renderer.text_style.active.textColor = Color.white;
            DebugHUD_Renderer.text_style.hover.textColor = Color.white;
            DebugHUD_Renderer.text_style.fontSize = 16;
            DebugHUD_Renderer.text_style.fontStyle = FontStyle.Bold;

            DebugHUD_Renderer.subtext_style = new GUIStyle(DebugHUD_Renderer.text_style);
            DebugHUD_Renderer.subtext_style.fontSize = 12;
            DebugHUD_Renderer.subtext_style.fontStyle = FontStyle.Normal;
            DebugHUD_Renderer.subtext_style.richText = true;
            
            DebugHUD_Renderer.skin = ScriptableObject.CreateInstance<GUISkin>();
            skin.name = "DebugHUD_Renderer";

            skin.verticalScrollbar = new GUIStyle();
            skin.verticalScrollbar.fixedWidth = scrollbar_width;
            Util.Set_BG_Color(skin.verticalScrollbar.normal, new Color32(16, 16, 16, 200));

            skin.verticalScrollbarThumb = new GUIStyle();
            skin.verticalScrollbarThumb.fixedWidth = scrollbar_width;
            Util.Set_BG_Color(skin.verticalScrollbarThumb.normal, new Color32(80, 80, 80, 255));

            skin.horizontalScrollbar = null;
            skin.horizontalScrollbarLeftButton = null;
            skin.horizontalScrollbarRightButton = null;
            skin.horizontalScrollbarThumb = null;


            DebugHUD_Renderer.console_text_style.normal.textColor = Color.white;
            DebugHUD_Renderer.console_text_style.normal.textColor = Color.white;
            DebugHUD_Renderer.console_text_style.active.textColor = Color.white;
            DebugHUD_Renderer.console_text_style.hover.textColor = Color.white;
            DebugHUD_Renderer.console_text_style.fontSize = 14;
            DebugHUD_Renderer.console_text_style.fontStyle = FontStyle.Normal;
            DebugHUD_Renderer.console_text_style.richText = true;


            watermark_style = new GUIStyle(GUI.skin.GetStyle("label"));
            watermark_style.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
            watermark_style.fontSize = 16;
            watermark_style.fontStyle = FontStyle.Bold;
            //title_style.normal.textColor = this.blue_clr;
            watermark_style.padding = new RectOffset(3, 3, 3, 3);
            watermark_style.normal.background = null;

            watermark_text_content.text = Loader.TITLE;
            watermark_content = new GUIContent(TextureHelper.icon_logo);
        }
        
        private void Update()
        {
            if (Player.gameObject != null)
            {
                update_player_pos_display();
            }

            if (Input.GetKeyUp(KeyCode.Tab) || (Input.GetKeyUp(KeyCode.Escape) && this.open))
            {
                this.open = (!this.open);
                if (this.open)
                {
                    this.new_count = 0;
                }
                this.onVisibility_Change(this.open);
            }
        }

        private void update_player_pos_display()
        {
            var pos = Player.gameObject.transform.position;
            player_pos_text.text = String.Format("Player: {0}, {1}, {2}", pos.x, pos.y, pos.z);

            float X = 2f;
            float Y = 2f;

            Vector2 posSz = console_text_style.CalcSize(player_pos_text);
            player_pos_area = new Rect(X, Y, posSz.x, posSz.y);
        }

        private void onVisibility_Change(bool is_vis)
        {
            if (is_vis == true) GameTime.Pause();
            else GameTime.Unpause();
        }
        
        private void doLayout()
        {
            this.needs_layout = false;
            float offsetH = 50f;
            float offsetBottom = 20f;
            float cPad = 5f;

            screen_area = new Rect(0f, 0f, Screen.width, Screen.height);
            fade_area = new Rect(0f, 0f, Screen.width, Screen.height);
            console_area = new Rect(cPad, offsetH, Screen.width - (cPad*2f), (Screen.height - offsetH - offsetBottom));

            float console_width = (console_area.width + scrollbar_width);//lest I ever change the width and neglect to also change the below text height calculation's width.
            float text_height = console_text_style.CalcHeight(console_lines, console_width);
            console_inner_area = new Rect(0f, 0f, console_width, text_height);
            console_inner_text_area = new Rect(console_inner_area.x+scrollbar_width+3f, console_inner_area.y, console_inner_area.width - scrollbar_width - 2f, text_height);
            //console_scrollbar_area = new Rect(0f, 0f, scrollbar_width, Screen.height);


            alertPos = new Rect(alert_icon_offset, alert_icon_offset, alert_size, alert_size);

            alert_txtSz = DebugHUD_Renderer.text_style.CalcSize(alert_content);
            alert_txtPos = new Rect(alertPos.x + alertPos.width + 3f, alertPos.y + (alertPos.height * 0.5f) - (alert_txtSz.y * 0.5f), alert_txtSz.x, alert_txtSz.y);
            alert_sub_txtSz = DebugHUD_Renderer.subtext_style.CalcSize(alert_sub_content);
            alert_sub_txtPos = new Rect(alert_txtPos.xMin, alert_txtPos.yMax + 3f, alert_sub_txtSz.x, alert_sub_txtSz.y);


            float hw = (Screen.width / 2f);
            var txtSZ = watermark_style.CalcSize(watermark_text_content);

            float logo_size = 36f;

            float pad = 2f;
            float X = pad;
            float Y = (Screen.height - (logo_size+pad));
            
            watermark_area = new Rect(X, Y, logo_size, logo_size);
            watermark_text_area = new Rect(watermark_area.xMax + 1f, watermark_area.yMax - (txtSZ.y - 2f), 300f, 25f);

        }
        /// <summary>
        /// Uses all of the system mouse movement, hover, and input events so we can prevent all controls under this one from getting them.
        /// Because this is a fullscreen overlay.
        /// </summary>
        private bool handleEvents()
        {
            id = GUIUtility.GetControlID(id, FocusType.Keyboard, screen_area);
            var evt = Event.current.GetTypeForControl(id);
            switch(evt)
            {
                case EventType.Layout:
                    this.doLayout();
                    return false;
                case EventType.Ignore:
                case EventType.Used:
                    return false;
                case EventType.Repaint:
                    break;
                case EventType.MouseDrag:
                case EventType.MouseMove:
                case EventType.MouseDown:
                case EventType.MouseUp:
                        //Event.current.Use();
                    return false;
                    break;
                default:
                        //Event.current.Use();
                    break;
            }

            return true;
        }

        private void OnGUI()
        {
            if (dirty_styles) this.Init_Styles();
            if(Event.current.GetTypeForControl(id) == EventType.Layout || this.needs_layout)
            {
                this.doLayout();
                return;
            }

            //this.Render_Loader_Watermark();
            if (!this.open)
            {
                if (this.new_count > 0)
                {
                    var clr = new Color(1f, 1f, 1f, 0.85f);
                    
                    //GUI.Label(alert_txtPos, alert_content, DebugHUD_Renderer.text_style);
                    DebugHUD_Renderer.text_style.Draw(alert_txtPos, alert_content, id);
                    DebugHUD_Renderer.subtext_style.Draw(alert_sub_txtPos, alert_sub_content, id);

                    var prevClr = GUI.color;
                    GUI.color = clr;

                    GUI.DrawTexture(alertPos, TextureHelper.icon_alert, ScaleMode.ScaleToFit);
                    GUI.color = prevClr;
                }
            }
            else
            {
                var prev_depth = GUI.depth;
                GUI.depth = 100;
                if (!this.handleEvents()) return;

                // darken the entire screen
                DebugHUD_Renderer.blackout.Draw(screen_area, GUIContent.none, id);
                // draw a black fade
                GUI.DrawTexture(fade_area, DebugHUD_Renderer.bg_fade, ScaleMode.StretchToFill);

                var prev_skin = GUI.skin;
                GUI.skin = skin;
                // draw the debug console text
                console_scroll = GUI.BeginScrollView(console_area, console_scroll, console_inner_area, false, false);
                    console_text_style.Draw(console_inner_text_area, console_lines, id);
                //PLog.Info("{0} {1} {2}", console_inner_area.height, console_inner_text_area.height, console_scrollbar_area.height);
                GUI.EndScrollView(true);

                GUI.depth = prev_depth;
                GUI.skin = prev_skin;

                console_text_style.Draw(player_pos_area, player_pos_text, false, false, false, false);
            }
        }

        private void Render_Loader_Watermark()
        {
            var prevClr = GUI.color;
            GUI.color = watermark_style.normal.textColor;
            GUI.DrawTexture(watermark_area, TextureHelper.icon_logo, ScaleMode.ScaleToFit);
            GUI.color = prevClr;
            //watermark_style.Draw(watermark_area, watermark_content, false, false, false, false);
            watermark_style.Draw(watermark_text_area, watermark_text_content, false, false, false, false);
        }

    }
}
