using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace SR_PluginLoader
{
    public class PluginManager : uiWindow
    {
        public static PluginManager Instance = null;

        private string selected = null;//the hash of the currently selected plugin
        private uiListView list = null;
        private uiTabPanel tabPanel = null;

        private uiText pl_title = null, pl_auth = null, pl_vers = null;
        private uiTextArea pl_desc = null;
        private uiIcon pl_thumb = null;
        private uiButton btn_copy_json = null;
        private uiToggle pl_toggle = null;
        private uiTextArea ins_title = null, ins_text = null, ins_no_plugins_text = null;
        private uiIcon btn_donate = null;
        private uiIconButton btn_store = null;
        private uiWrapperPanel nop_wrapper = null, top_wrapper;
        private uiTab pl_tab = null, tab_need_plugins = null, tab_ins = null;
        private uiCollapser control_panel = null, pl_errors=null;

        const string PLUGIN_TAB_NAME = "plugins_tab";
        const string INSTRUCTION_TAB_NAME = "instructions_tab";
        const string NEED_PLUGINS_TAB_NAME = "need_plugins_tab";

        public PluginManager()
        {
            onLayout += PluginManager_onLayout;
            Title = "Plugin Manager";
            Set_Size(650, 400);
            Center();
            onShown += PluginManager_onShown;
            onHidden += PluginManager_onHidden;

            list = Create<uiListView>(this);
            list.Set_Width(200f);
            list.Set_Margin(2, 0, 2, 2);


            top_wrapper = Create<uiWrapperPanel>(this);
            top_wrapper.Autosize_Method = AutosizeMethod.BLOCK;
            top_wrapper.Set_Margin(0, 0, 0, 2);
            top_wrapper.Set_Padding(2);
            top_wrapper.onLayout += Top_wrapper_onLayout;


            btn_store = Create<uiIconButton>(top_wrapper);
            btn_store.Text = "Plugin Store";
            btn_store.Icon = TextureHelper.icon_arrow_left;
            btn_store.Border.type = uiBorderType.NONE;
            btn_store.Border.normal.color = Color.white;
            btn_store.Border.normal.size = new RectOffset(1, 1, 1, 1);
            btn_store.onClicked += (uiControl c) => { uiWindow.Switch(PluginStore.Instance); };


            btn_donate = Create<uiIcon>(top_wrapper);
            btn_donate.Text = null;
            btn_donate.Image = (Texture2D)TextureHelper.Load_From_Resource("donate_btn.png", "SR_PluginLoader", (TextureOpFlags.NO_MIPMAPPING & TextureOpFlags.NO_WRAPPING));
            btn_donate.Image_MouseOver = Util.Tint_Texture( (Texture2D)TextureHelper.Load_From_Resource("donate_btn.png", "SR_PluginLoader", (TextureOpFlags.NO_MIPMAPPING & TextureOpFlags.NO_WRAPPING)), new Color(0.7f,0.7f,0.7f));
            btn_donate.Border.type = uiBorderType.NONE;
            btn_donate.onClicked += (uiControl c) => { Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DYGPA5XA4MWC2"); };


            //CONTROL PANEL
            control_panel = Create<uiCollapser>("control_panel", this);
            control_panel.Autosize_Method = AutosizeMethod.BLOCK;
            control_panel.Set_Margin(2, 2, 2, 0);
            control_panel.Set_Padding(2);
            Util.Set_BG_Color(control_panel.local_style.normal, new Color(0f, 0f, 0f, 0.2f));
            control_panel.onLayout += Control_panel_onLayout;
            control_panel.Collapse();


            pl_toggle = Create<uiToggle>("pl_toggle", control_panel);
            pl_toggle.onChange += Pl_toggle_onChange;

            btn_copy_json = Create<uiButton>("copy_json", control_panel);
            btn_copy_json.Text = "Copy Json";
            btn_copy_json.Set_Margin(3, 0);
            btn_copy_json.onClicked += Btn_copy_json_onClicked;


            // TABBED PANEL
            tabPanel = Create<uiTabPanel>(this);
            tabPanel.Autosize_Method = AutosizeMethod.FILL;
            tabPanel.disableBG = true;
            tabPanel.Set_Margin(2);// This margin gives us that light colored area surrounding the list and tabpanel
            pl_tab = tabPanel.Add_Tab(PLUGIN_TAB_NAME);
            tabPanel.CurrentTab.onLayout += tabPanel_onLayout;
            tabPanel.onChanged += TabPanel_onChanged;

            pl_tab.Set_Padding(3);// so we can distinguish where the plugin thumbnail's borders are


            // PLUGIN ERRORS PANEL
            pl_errors = Create<uiCollapser>(pl_tab);
            pl_errors.onLayout += Clps_errors_onLayout;
            pl_errors.Collapse();

            var err_ico = Create<uiIcon>("err_ico", pl_errors);
            err_ico.Image = TextureHelper.icon_alert;
            err_ico.Set_Height(26);

            var err_lbl = Create<uiText>("err_lbl", pl_errors);
            err_lbl.TextColor = Color.red;
            err_lbl.Text = "Plugin experienced errors while loading!";
            err_lbl.TextAlign = TextAnchor.MiddleLeft;
            err_lbl.Set_Margin(2, 0, 0, 0);


            // PLUGIN INFO

            pl_title = Create<uiText>("pl_title", pl_tab);
            pl_title.local_style.fontSize = 22;
            pl_title.local_style.fontStyle = FontStyle.Bold;


            pl_auth = Create<uiText>("pl_author", pl_tab);
            pl_vers = Create<uiText>("pl_vers", pl_tab);
            pl_desc = Create<uiTextArea>("pl_desc", pl_tab);

            pl_thumb = Create<uiIcon>("pl_thumb", pl_tab);
            pl_thumb.Border.normal.color = new Color(0f, 0f, 0f, 0.3f);

            pl_desc.Set_Margin(2);
            pl_desc.Set_Background(new Color(0.1f, 0.1f, 0.1f, 0.4f));
            pl_desc.Set_Padding(2);
            pl_desc.Border.normal.color = new Color(0f, 0f, 0f, 0.3f);
            pl_desc.Autosize_Method = AutosizeMethod.BLOCK;

            // INSTRUCTIONS TAB
            tab_ins = tabPanel.Add_Tab(INSTRUCTION_TAB_NAME);
            tabPanel.CurrentTab.onLayout += InstructionsTab_onLayout;

            ins_title = Create<uiTextArea>(tabPanel);
            ins_title.Text = "Settings";
            ins_title.TextSize = 20;
            ins_title.TextStyle = FontStyle.Bold;

            ins_text = Create<uiTextArea>(tabPanel);
            ins_text.Text = "Select a plugin from the list on the left to manage it's settings.\nOr, to browse more plugins for download, click the \"Plugin Store\" button at the top left of this window!";
            ins_text.TextStyle = FontStyle.Italic;
            ins_text.Autosize = true;


            tab_need_plugins = tabPanel.Add_Tab(NEED_PLUGINS_TAB_NAME);
            tabPanel.CurrentTab.onLayout += NeedPluginsTab_onLayout;

            nop_wrapper = Create<uiWrapperPanel>(tabPanel);
            nop_wrapper.onLayout += Nop_Wrapper_onLayout;

            ins_no_plugins_text = Create<uiTextArea>(nop_wrapper);
            ins_no_plugins_text.Text = "You do not have any plugins installed!\nVisit the plugin store to find and install new plugins.";
            ins_no_plugins_text.TextSize = 16;
            ins_no_plugins_text.TextColor = new Color(1f, 0.1f, 0.1f, 0.9f);
            ins_no_plugins_text.TextStyle = FontStyle.Bold;
            ins_no_plugins_text.TextAlign = TextAnchor.UpperCenter;
            ins_no_plugins_text.Autosize = true;
        }

        private void Top_wrapper_onLayout(uiPanel c)
        {
            btn_store.alignLeftSide();
            btn_donate.alignRightSide();
            btn_donate.Set_Height(btn_store.Get_Height());// eh, match the store buttons height...
        }

        private void Clps_errors_onLayout(uiPanel c)
        {
            var err_ico = c["err_ico"];
            var err_lbl = c["err_lbl"];
            
            err_lbl.moveRightOf(err_ico);
            err_lbl.CenterVertically();
        }

        private void PluginManager_onLayout(uiPanel c)
        {
            list.alignLeftSide();

            control_panel.moveBelow(top_wrapper);
            control_panel.moveRightOf(list);

            list.moveBelow(top_wrapper);
            list.FloodY();

            tabPanel.moveBelow(control_panel);
            tabPanel.moveRightOf(list);
            tabPanel.FloodXY();
        }

        private void TabPanel_onChanged(uiTabPanel arg1, uiTab tab)
        {
            control_panel.Set_Collapsed(tab != pl_tab);
        }

        private void Control_panel_onLayout(uiControl c)
        {
            pl_toggle.alignLeftSide();
            btn_copy_json.moveRightOf(pl_toggle, 3);
        }
        
        private void Nop_Wrapper_onLayout(uiControl c)
        {
            ins_no_plugins_text.Set_Pos(0, 0);
        }

        private void NeedPluginsTab_onLayout(uiControl c)
        {
            nop_wrapper.CenterVertically();
            nop_wrapper.CenterHorizontally();
        }

        private void InstructionsTab_onLayout(uiControl c)
        {
            ins_title.alignTop(5);
            ins_title.CenterHorizontally();

            ins_text.moveBelow(ins_title, 6);
            ins_text.CenterHorizontally();
        }

        private void tabPanel_onLayout(uiControl c)
        {
            pl_errors.alignTop();
            pl_errors.CenterHorizontally();

            //pl_thumb.alignTop();
            pl_thumb.moveBelow(pl_errors, 2);
            pl_thumb.CenterHorizontally();
            
            //pl_toggle.CenterHorizontally();
            //pl_toggle.moveBelow(pl_thumb, 5);
            
            pl_title.moveBelow(pl_thumb, 10);
            pl_title.CenterHorizontally();

            pl_vers.moveBelow(pl_title, 1);
            pl_vers.CenterHorizontally();

            pl_auth.moveBelow(pl_vers, 1);
            pl_auth.CenterHorizontally();

            pl_desc.alignLeftSide(3);
            pl_desc.moveBelow(pl_auth, 10);

            //btn_copy_json.moveBelow(pl_desc, 5);
        }

        private void Pl_toggle_onChange(uiToggle c, bool was_clicked)
        {
            if (!was_clicked) return;//if the control didnt change due to being clicked then it changed because WE manually set the state. which we don't want to react to or we will enter a loop.

            if (c.isChecked) GetPlugin().Enable();
            else GetPlugin().Disable();
        }

        private void Btn_copy_json_onClicked(uiControl c)
        {
            Plugin pl = Plugin.Get_Plugin(this.selected);
            if (pl == null)
            {
                SLog.Info("Unable to find plugin via hash: {0}", this.selected);
                return;
            }
            GUIUtility.systemCopyBuffer = pl.data.ToJSON();
        }
        
        private void PluginManager_onShown(uiControl c)
        {
            Update_Plugins_List();
        }
        
        private void PluginManager_onHidden(uiWindow c)
        {
            //tabPanel.Set_Tab(INSTRUCTION_TAB_NAME);
            tab_ins.Select();
        }

        public void Update_Plugins_List()
        {
            set_layout_dirty();
            list.Clear_Children();

            float dY = 3f;
            foreach (KeyValuePair<string, Plugin> kv in Loader.plugins)
            {
                try
                {
                    var sel = Create<Plugin_Manager_List_Item>();
                    sel.Set_Plugin(kv.Value);
                    sel.onClicked += Sel_onClicked;
                    list.Add(kv.Value.Hash, sel);
                }
                catch (Exception ex)
                {
                    SLog.Error(ex);
                }
            }

            if (list.isEmpty) tab_need_plugins.Select();// tabPanel.Set_Tab(NEED_PLUGINS_TAB_NAME);
            else tab_ins.Select();// tabPanel.Set_Tab(INSTRUCTION_TAB_NAME);
            // Set the very first plugin in our list as the active one
            //if (Loader.plugins.Count > 0) { this.Select_Plugin(Loader.plugins.First().Value); }
        }

        private void Sel_onClicked(uiControl c)
        {
            Plugin_Manager_List_Item sel = c as Plugin_Manager_List_Item;
            Select_Plugin(sel.Get_Plugin());
        }

        private void Select_Plugin(Plugin p)
        {
            set_layout_dirty();
            if (!list.isEmpty)
            {
                pl_tab.Select();
                //set ALL selectors to inactive first.
                foreach (var sel in list.Get_Children())
                {
                    ((Plugin_Manager_List_Item)sel).Active = false;
                }
            }
            else
            {
                tab_ins.Select();
            }

            // Unregister our error event hook for the last plugin we had selected
            Plugin last = GetPlugin();
            if (last != null) last.onError -= onPluginError;

            selected = null;
            if (p != null)
            {
                selected = p.Hash;
                Plugin_Manager_List_Item sel = list[p.Hash] as Plugin_Manager_List_Item;
                if(sel != null) sel.Active = true;
            }
            p.onError += onPluginError;

            control_panel.Set_Collapsed(selected==null);

            float thumb_aspect = 1f;
            float thumb_sz = 256f;

            if (p == null || p.data == null)
            {
                this.pl_title.Text = "";
                this.pl_auth.Text = "";
                this.pl_desc.Text = "";
                //this.pl_desc.isDisabled = true;

                this.pl_vers.Text = "";
                this.pl_thumb.Image = null;
                this.pl_toggle.isVisible = false;
                this.pl_toggle.isChecked = false;
                this.pl_errors.Collapse();
            }
            else
            {
                this.pl_title.Text = p.data.NAME;
                this.pl_auth.Text = String.Format("<color=#808080ff>Author:</color> {0}", p.data.AUTHOR);
                this.pl_desc.Text = (string.IsNullOrEmpty(p.data.DESCRIPTION) ? "<b><color=#808080ff>No Description</color></b>" : p.data.DESCRIPTION);
                //this.pl_desc.isDisabled = false;

                this.pl_vers.Text = p.data.VERSION.ToString();
                this.pl_thumb.Image = p.thumbnail;
                this.pl_toggle.isVisible = true;
                this.pl_toggle.isChecked = p.Enabled;
                this.pl_errors.Set_Collapsed(!p.HasErrors);
            }

            if (this.pl_thumb.Image == null) thumb_sz = 0f;
            else thumb_aspect = ((float)this.pl_thumb.Image.height / (float)this.pl_thumb.Image.width);

            float thumb_height = (thumb_sz * thumb_aspect);
            pl_thumb.Set_Size(thumb_sz, thumb_height);
        }

        private void onPluginError() { pl_errors.Expand(); }

        private Plugin GetPlugin()
        {
            return Plugin.Get_Plugin(this.selected);
        }

    }
}
