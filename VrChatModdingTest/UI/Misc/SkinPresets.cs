using System;
using UnityEngine;

namespace SR_PluginLoader
{
    public static class SkinPresets
    {
        private static GUIStyle defaultFont(GUIStyle style, Color color, int font_size=14)
        {
            style.clipping = TextClipping.Overflow;
            //style.clipping = TextClipping.Clip;// Clipping results in ugly text usually.
            //style.alignment = TextAnchor.MiddleLeft;
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = font_size;

            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;

            return style;
        }

        private static GUIStyle Create_Named_Style(string name)
        {
            GUIStyle style = new GUIStyle();
            style.name = name;
            return style;
        }

        private static GUISkin New_Skin()
        {
            GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
            skin.customStyles = new GUIStyle[] {
                Create_Named_Style("panel"),
                Create_Named_Style("disabled"),
            };

            try
            {
                var style_panel = skin.GetStyle("panel");
                
                // Setup all of the default font values
                skin.box = defaultFont(skin.box, Color.white);
                skin.button = defaultFont(skin.button, Color.white);
                skin.window = defaultFont(skin.window, new Color(0.7f, 0.7f, 0.7f));
                skin.label = defaultFont(skin.label, Color.white);
                skin.textArea = defaultFont(skin.textArea, Color.white);
                skin.textField = defaultFont(skin.textField, Color.white);
                skin.scrollView = defaultFont(skin.scrollView, Color.white);
                style_panel = defaultFont(style_panel, Color.white);
                
                skin.textArea.wordWrap = true;//TextAreas are multi-line by default
                skin.textField.wordWrap = true;

                skin.textArea.alignment = TextAnchor.UpperLeft;
                skin.label.alignment = TextAnchor.MiddleLeft;
                skin.button.alignment = TextAnchor.MiddleCenter;

                skin.window.fontSize = 16;
                skin.button.fontSize = 16;

                skin.button.fontStyle = FontStyle.Bold;

                // Text Editor
                skin.settings.cursorFlashSpeed = 0.8f;
                skin.settings.selectionColor = new Color(0.196f, 0.592f, 0.992f, 0.5f);

                // Scrollbars
                int scrollbar_width = 9;
                skin.verticalScrollbar = new GUIStyle();
                skin.verticalScrollbar.fixedWidth = scrollbar_width;
                Util.Set_BG_Color(skin.verticalScrollbar.normal, new Color32(16, 16, 16, 200));

                skin.verticalScrollbarThumb = new GUIStyle();
                skin.verticalScrollbarThumb.fixedWidth = scrollbar_width;
                Util.Set_BG_Color(skin.verticalScrollbarThumb.normal, new Color32(80, 80, 80, 255));
                
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
            }

            return skin;
        }

        public static GUISkin Create_Default()
        {
            GUISkin skin = New_Skin();
            
            var style_panel = skin.GetStyle("panel");
            var style_disabled_overlay = skin.GetStyle("disabled");


            Util.Set_BG_Color(style_disabled_overlay.normal, new Color(0.05f, 0.05f, 0.05f, 0.45f));
            Util.Set_BG_Color(skin.box.normal, new Color(0f, 0f, 0f, 0.2f));
            Util.Set_BG_Color(skin.window.normal, new Color32(50, 50, 50, 255));

            byte g = 20;
            Util.Set_BG_Color(skin.textField.normal, new Color32(g, g, g, 255));


            // We want a sheen texture for buttons
            //skin.button.normal.background = Utility.Create_Sheen_Texture(100, Color.white);
            //Utility.Set_BG_Color(skin.button.normal, new Color32(32, 32, 32, 180));
            //Util.Set_BG_Gradient(skin.button.normal, 100, GRADIENT_DIR.TOP_BOTTOM, new Color32(45, 45, 45, 180), new Color32(32, 32, 32, 180));

            // Nevermind let's go with a bluish color for buttons by default.
            Color blue = new Color(0.2f, 0.4f, 1f, 1f);
            Util.Set_BG_Gradient(skin.button.normal, 64, GRADIENT_DIR.TOP_BOTTOM, blue, 0.5f);
            Util.Set_BG_Gradient(skin.button.hover, 64, GRADIENT_DIR.TOP_BOTTOM, blue.Mul(0.8), 0.5f);

            return skin;
        }

        public static GUISkin Create_Flat()
        {
            GUISkin skin = New_Skin();
            var style_panel = skin.GetStyle("panel");
            var style_disabled_overlay = skin.GetStyle("disabled");


            Util.Set_BG_Color(style_disabled_overlay.normal, new Color(0.05f, 0.05f, 0.05f, 0.45f));
            Util.Set_BG_Color(skin.box.normal, new Color(0f, 0f, 0f, 0.2f));
            Util.Set_BG_Color(skin.window.normal, new Color32(50, 50, 50, 255));

            byte g = 20;
            Util.Set_BG_Color(skin.textField.normal, new Color32(g, g, g, 255));

            Util.Set_BG_Color(skin.button.normal, new Color(0f, 0f, 0f, 0.3f));
            Util.Set_BG_Color(skin.button.hover, new Color(0.15f, 0.15f, 0.15f, 0.3f));

            return skin;
        }
    }
}
