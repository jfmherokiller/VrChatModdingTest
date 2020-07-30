using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SR_PluginLoader
{
    public class Plugin_Manager_List_Item : uiPanel
    {
        public string Hash { get { if (this.plugin == null) { return null; } return this.plugin.Hash; } }
        private Plugin plugin = null;
        private uiText pl_title = null, pl_version = null, pl_status = null;
        private uiIcon pl_icon = null;

        private bool plugin_state_init = false;
        private bool last_plugin_en_state = false;
        

        public Plugin_Manager_List_Item() : base()
        {
            Autosize_Method = AutosizeMethod.BLOCK;
            Set_Height(50);
            Set_Margin(4, 4, 2, 0);
            Set_Padding(2, 0, 2, 0);
            Border.normal = new uiBorderStyleState() { color = new Color32(10, 10, 10, 255) };
            //Border.hover = new uiBorderStyleState() { color = new Color32(255, 255, 255, 255) };

            float shade = 0.2f;
            //set normal styles bg color
            
            Util.Set_BG_Color(local_style.normal, shade, shade, shade, 0.9f);
            local_style.active.background = Util.Get_Gradient_Texture(200, GRADIENT_DIR.TOP_BOTTOM, new Color(shade, shade, shade), new Color32(25, 99, 141, 255));

            shade += 0.3f;
            Util.Set_BG_Color(local_style.hover, shade, shade, shade, 0.5f);


            pl_icon = Create<uiIcon>("icon", this);

            pl_title = Create<uiText>("title", this);
            pl_title.TextColor = new Color(1f, 1f, 1f);
            pl_title.TextStyle = FontStyle.Bold;
            pl_title.TextSize = 16;

            pl_version = Create<uiText>("version", this);
            pl_version.TextColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
            pl_version.TextStyle = FontStyle.Italic;
            pl_version.TextSize = 12;

            pl_status = Create<uiText>("status", this);
            Util.Set_BG_Color(pl_status.local_style.normal, 0f, 0f, 0f, 0.3f);
            pl_status.Set_Padding(2, 1);
            pl_status.TextStyle = FontStyle.Bold;
            pl_status.TextSize = 12;
            pl_status.TextAlign = TextAnchor.MiddleCenter;
            //pl_status.local_style.normal.textColor = Color.red;// plugin is disabled
            //pl_status.local_style.active.textColor = Color.green;// plugin is enabled
        }

        private void Update()
        {
            if (plugin == null) return;
            if (plugin.Enabled != last_plugin_en_state || !plugin_state_init)
            {
                plugin_state_init = true;
                last_plugin_en_state = plugin.Enabled;
                Plugin_State_Changed();
            }
        }

        private void Plugin_State_Changed()
        {
            //this.pl_status.Active = plugin.enabled;
            pl_status.TextColor = (plugin.Enabled ? Color.green : Color.red);
            pl_status.Text = (plugin.Enabled ? "Enabled" : "Disabled");
        }

        public override void doLayout()
        {
            base.doLayout();
            float icon_sz = this.inner_area.height;
            pl_icon.Set_Pos(0, 0);
            pl_icon.Set_Size(icon_sz, icon_sz);

            const float xPad = 4f;
            pl_title.moveRightOf(pl_icon, xPad);

            pl_version.moveRightOf(pl_icon, xPad);
            pl_version.moveBelow(pl_title);

            pl_status.alignBottom();
            pl_status.alignRightSide();
        }

        public Plugin Get_Plugin() { return this.plugin; }

        public void Set_Plugin(Plugin p)
        {
            if (p == null)
            {
                SLog.Error(new Exception("Plugin is null!"));
                return;
            }

            try
            {
                plugin = p;
                if (plugin.data == null)
                {
                    SLog.Error(new Exception("Plugin data is NULL!"));
                    return;
                }

                pl_title.Text = plugin.data.NAME;
                pl_version.Text = plugin.data.VERSION.ToString();
                Plugin_State_Changed();

                if (plugin.icon != null) pl_icon.Image = plugin.icon;
                else if (TextureHelper.icon_unknown != null) pl_icon.Image = TextureHelper.icon_unknown;
                else pl_icon.Image = null;

                plugin_state_init = false;
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
            }
        }

    }
}
