using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class Plugin_StoreItem : uiPanel
    {
        public static float DEFAULT_WIDTH = 180f;
        public static float DEFAULT_HEIGHT = 48f;

        private string _plugin_hash = null;
        public string plugin_hash { get { return _plugin_hash; } }

        public uiProgressBar progress_bar = null;
        public uiVarText progress_text = null;
        public uiCollapser download_info = null;
        public override string Text { get { return this["name"].Text; } set { this["name"].Text = value; } }


        public Plugin_StoreItem() : base(uiControlType.Panel)
        {
            Set_Padding(4, 2, 2, 2);
            Set_Margin(2, 2, 2, 2);
            //Set_Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            Autosize_Method = AutosizeMethod.BLOCK;
            
            Util.Set_BG_Color(local_style.normal, new Color32(32, 32, 32, 200));
            Util.Set_BG_Color(local_style.hover, new Color32(64, 64, 64, 255));
            const float b = 0.5f;
            Border.normal.color = new Color(b, b, b, 1f);

            const float g = 0.8f;
            TextColor = new Color(g, g, g, 1f);
            TextColor_Hover = Color.white;
            //Utility.Set_BG_Color(local_style.hover, new Color32(32, 40, 60, 255));
            
            uiText name = uiControl.Create<uiText>("name", this);
            name.Clone_Text_Style(this);
            name.TextStyle = FontStyle.Bold;
            name.TextSize = 14;
            name.Autosize_Method = AutosizeMethod.BLOCK;
            
            uiText auth = uiControl.Create<uiText>("author", this);
            auth.Clone_Text_Style(this);
            auth.TextStyle = FontStyle.Normal;
            auth.TextSize = 12;
            auth.Autosize_Method = AutosizeMethod.BLOCK;

            download_info = Create<uiCollapser>("download_info", this);
            download_info.Size_Height_Collapsed = 0;
            download_info.onLayout += Download_info_onLayout;
            download_info.Set_Margin(0);
            download_info.Set_Padding(0);
            //Util.Set_BG_Color(download_info.local_style.normal, new Color(0.1f, 0.5f, 1.0f));
            download_info.Set_Collapsed(true);

            progress_bar = Create<uiProgressBar>("progress", download_info);
            progress_bar.Set_Height(4f);
            progress_bar.Value = 0;
            progress_bar.show_progress_text = false;
            Util.Set_BG_Color(progress_bar.prog_bar.local_style.normal, new Color(0.1f, 0.5f, 1.0f));
            Util.Set_BG_Color(progress_bar.local_style.normal, new Color(0.1f, 0.1f, 0.1f));


            progress_text = uiControl.Create<uiVarText>("progress_text", download_info);
            progress_text.Clone_Text_Style(this);
            progress_text.TextAlign = TextAnchor.LowerRight;
            progress_text.TextStyle = FontStyle.Normal;
            progress_text.TextSize = 12;
            progress_text.Text = "Downloading:";
            //progress_text.Text = "0%";
        }

        private void Download_info_onLayout(uiControl c)
        {
            progress_text.alignLeftSide();
            progress_text.alignTop();

            progress_bar.moveBelow(progress_text, 2);
            progress_bar.FloodX();
        }

        public void Set_Plugin_Data(Plugin_Data data)
        {
            _plugin_hash = data.Hash;
            this["name"].Text = data.NAME;
            this["author"].Text = data.AUTHOR;
            progress_bar.Value = 0f;
        }

        public override void doLayout()
        {
            base.doLayout();
            uiControl name = this["name"];
            uiControl auth = this["author"];

            //name.area = new Rect(0f, 0f, area.width, name.Style.lineHeight);
            //auth.area = new Rect(10f, name.area.yMax, area.width, name.Style.lineHeight);
            name.alignTop();
            name.alignLeftSide();

            auth.moveBelow(name, 0);
            auth.alignLeftSide(8);

            download_info.alignLeftSide();
            download_info.moveBelow(auth, 0f);
            download_info.FloodX();
        }
    }
}
