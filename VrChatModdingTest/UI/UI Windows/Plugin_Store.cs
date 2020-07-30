using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SimpleJSON;
using System.Net;
using System.IO;

namespace SR_PluginLoader
{
    /// <summary>
    /// A "form" that displays the plugins in the master list from the github page
    /// Users can view all the plugins and pick ones to download & install!
    /// </summary>
    public class PluginStore : uiWindow
    {
        public static PluginStore Instance = null;

        private readonly string PLUGINS_LIST_URL = "https://github.com/dsisco11/SR_Plugin_Loader/raw/master/MASTER_PLUGINS_LIST.json";
        private Dictionary<string, Plugin_Download_Data> plugins = new Dictionary<string, Plugin_Download_Data>();
        private uiListView list = null;
        private uiTextbox search = null;
        private uiText lbl_search = null, pl_title = null, pl_auth = null;
        private uiVarText lbl_pl_count = null;
        private uiTextArea pl_desc = null;
        private bool loaded = false, pending_rebuild = false;
        private Plugin_StoreItem selected_plugin = null;
        private uiButton install_btn = null, retry_pl_list_btn = null;
        private uiTabPanel tabPanel = null;
        private uiText ins_title = null, ins_text = null;
        private uiTabPanel statusPanel = null;
        private uiTab loading_tab = null, plugins_tab = null;
        private uiTextArea ld_text = null;
        private uiIconButton btn_config = null;

        const string MAIN_TAB_NAME = "main_tab";
        const string INSTRUCTION_TAB_NAME = "instructions_tab";


        public PluginStore()
        {
            onLayout += PluginStore_onLayout;
            Title = "Plugin Store";
            Set_Size(800, 600);
            Center();


            lbl_pl_count = Create<uiVarText>(this);
            lbl_pl_count.Text = "Total Plugins:";
            lbl_pl_count.Value = "0";
            lbl_pl_count.text_style.fontStyle = FontStyle.Bold;

            search = uiControl.Create<uiTextbox>(this);
            search.onChange += Search_onChange;
            
            lbl_search = uiControl.Create<uiText>(this);
            lbl_search.Text = "Search ";
            

            btn_config = Create<uiIconButton>(this);
            btn_config.Text = "Settings";
            btn_config.Icon = TextureHelper.icon_arrow_left;
            btn_config.Border.type = uiBorderType.NONE;
            //btn_config.Skin = uiSkinPreset.FLAT;
            btn_config.Border.normal.color = Color.white;
            btn_config.Border.normal.size = new RectOffset(1, 1, 1, 1);
            btn_config.onClicked += Btn_config_onClicked;


            tabPanel = Create<uiTabPanel>(this);
            tabPanel.Add_Tab(MAIN_TAB_NAME);
            tabPanel.CurrentTab.onLayout += InfoTab_onLayout;
            tabPanel.Set_Margin(2);// This margin gives us that light colored area surrounding the list and tabpanel
            

            pl_title = uiControl.Create<uiText>(tabPanel);
            pl_title.Text = "";
            var sty = new GUIStyle(pl_title.Style);
            sty.fontStyle = FontStyle.Bold;
            sty.fontSize = 22;
            pl_title.Set_Style(sty);


            pl_auth = uiControl.Create<uiText>(tabPanel);
            pl_auth.Text = "";

            pl_desc = uiControl.Create<uiTextArea>(tabPanel);
            pl_desc.Text = "";
            pl_desc.Set_Padding(2);
            pl_desc.Border.normal.color = new Color(0f, 0f, 0f, 0.3f);


            install_btn = uiControl.Create<uiButton>(tabPanel);
            install_btn.local_style.fontSize = 18;
            install_btn.local_style.fontStyle = FontStyle.Bold;
            install_btn.Text = "Download";
            install_btn.onClicked += Install_btn_onClicked;
            Color blue = new Color(0.2f, 0.4f, 1f, 1f);
            Util.Set_BG_Gradient(install_btn.local_style.normal, 64, GRADIENT_DIR.TOP_BOTTOM, blue, new Color(0.5f, 0.5f, 0.5f, 1f) * blue);


            // INSTRUCTIONS TAB
            tabPanel.Add_Tab(INSTRUCTION_TAB_NAME);
            tabPanel.CurrentTab.onLayout += InstructionsTab_onLayout; ;

            ins_title = Create<uiTextArea>(tabPanel);
            ins_title.Text = "Welcome";
            ins_title.TextSize = 22;
            ins_title.TextStyle = FontStyle.Bold;

            ins_text = Create<uiTextArea>(tabPanel);
            ins_text.Text = "Here you can find and download new plugins.\nTo get started select a plugin from the list on the left to see more information about it.\nWhen you find a plugin you wish to install you can do so by clicking the \"Install\" button!";
            ins_text.TextStyle = FontStyle.Italic;
            ins_text.TextAlign = TextAnchor.UpperCenter;
            ins_text.Autosize = true;

            statusPanel = Create<uiTabPanel>(this);
            statusPanel.Set_Width(200);
            statusPanel.Set_Margin(2, 0, 2, 2);
            statusPanel.onLayout += StatusPanel_onLayout;

            loading_tab = statusPanel.Add_Tab("loading_tab");
            ld_text = Create<uiTextArea>("ld_text", statusPanel);
            ld_text.Text = "...";
            ld_text.TextSize = 16;
            ld_text.TextAlign = TextAnchor.MiddleCenter;
            
            retry_pl_list_btn = Create<uiButton>(statusPanel);
            retry_pl_list_btn.Set_Margin(0, 0, 4, 0);
            retry_pl_list_btn.Text = "Refresh";
            retry_pl_list_btn.TextSize = 16;
            Util.Set_BG_Gradient(retry_pl_list_btn.local_style.normal, 64, GRADIENT_DIR.TOP_BOTTOM, blue, new Color(0.5f, 0.5f, 0.5f, 1f) * blue);
            retry_pl_list_btn.onClicked += retry_btn_onClicked;
            retry_pl_list_btn.isVisible = false;

            statusPanel.CurrentTab.onLayout += CurrentTab_onLayout;

            plugins_tab = statusPanel.Add_Tab("plugins_tab");
            list = Create<uiListView>(plugins_tab);
            list.disableBG = true;
            list.Autosize_Method = AutosizeMethod.FILL;
            list.FloodXY();

            loading_tab.Select();
        }

        private void PluginStore_onLayout(uiPanel c)
        {
        }

        public override void doLayout()
        {
            base.doLayout();

            btn_config.alignTop();
            btn_config.alignLeftSide();

            float xPad = 5f;
            lbl_search.alignTop(10f);
            lbl_search.moveRightOf(statusPanel, xPad);

            search.alignTop(10f);
            search.moveRightOf(lbl_search);
            search.FloodX(12f);
            if (search.Area.size.y != lbl_search.Area.size.y) lbl_search.Set_Height(search.Area.size.y);

            statusPanel.moveBelow(search, 10f);
            statusPanel.FloodY();

            lbl_pl_count.alignLeftSide(5f);
            lbl_pl_count.moveBelow(btn_config, 2f);

            tabPanel.moveBelow(search, 10f);
            tabPanel.moveRightOf(statusPanel, 2f);
            tabPanel.FloodXY();
        }

        private void Btn_config_onClicked(uiControl c)
        {
            uiWindow.Switch(PluginManager.Instance);
        }

        private void StatusPanel_onLayout(uiControl c)
        {
            //uiText ld_text = (uiText)loading_tab["ld_text"];
            ld_text.CenterVertically();
            ld_text.CenterHorizontally();

            retry_pl_list_btn.moveBelow(ld_text);
            retry_pl_list_btn.CenterHorizontally();
        }

        private void CurrentTab_onLayout(uiControl c)
        {
            loading_tab.CenterVertically();
            loading_tab.CenterHorizontally();
        }

        private void InstructionsTab_onLayout(uiControl c)
        {
            ins_title.alignTop(5);
            ins_title.CenterHorizontally();

            ins_text.CenterHorizontally();
            ins_text.moveBelow(ins_title, 6);
        }

        private void InfoTab_onLayout(uiControl c)
        {
            install_btn.alignTop(4);
            install_btn.CenterHorizontally();

            pl_title.moveBelow(install_btn, 10f);
            pl_title.CenterHorizontally();

            pl_auth.moveBelow(pl_title, 2f);
            pl_auth.CenterHorizontally();

            pl_desc.moveBelow(pl_auth, 5f);
        }
        
        private void Install_btn_onClicked(uiControl c)
        {
            if (selected_plugin == null)
            {
                Sound.Play(SoundId.NEGATIVE);
                return;
            }
            var hash = selected_plugin.plugin_hash;
            string url = this.plugins[hash].URL;
            if(url == null || url.Length <= 0)
            {
                SLog.Info("Cannot download plugin, Invalid URL: {0}", url);
                return;
            }
            
            Download_Plugin(hash, url);
        }

        private void Search_onChange(uiControl c, string str)
        {
            pending_rebuild = true;
        }
       
        private void Select_Plugin(uiControl c)
        {
            Plugin_StoreItem pl = (Plugin_StoreItem)c;
            if (selected_plugin != null)
                selected_plugin.Active = false;
            selected_plugin = pl;

            if (c != null) tabPanel.Select_Tab(MAIN_TAB_NAME);
            else tabPanel.Select_Tab(INSTRUCTION_TAB_NAME);

            if (c == null)
            {
                pl_title.Text = "";
                pl_auth.Text = "N/A";
                pl_desc.Text = "N/A";
                install_btn.isDisabled = true;
                return;
            }
            else install_btn.isDisabled = false;
            c.Active = true;
            Plugin_Download_Data data = this.plugins[pl.plugin_hash];

            pl_title.Text = data.Name;
            pl_auth.Text = String.Format("<color=#808080ff>Author:</color> {0}", data.Author);
            pl_desc.Text = data.Description;
            if (pl_desc.Text == null || pl_desc.Text.Length <= 0) pl_desc.Text = "N/A";

            //install_btn.isVisible = !data.isInstalled;
            install_btn.isDisabled = data.isInstalled;
            install_btn.Text = data.isInstalled ? "Installed" : "Download";
        }

        public void Update()
        {
            if (pending_rebuild) Rebuild_Plugins_UI();
        }

        public override void handle_FirstFrame()
        {
            base.handle_FirstFrame();
            Update_Plugins_List();
        }
        

        private void retry_btn_onClicked(uiControl c)
        {
            Update_Plugins_List();
        }

        private void Update_Plugins_List()
        {
            loaded = true;
            StartCoroutine(Client_Plugins_List_Update());
        }
        
        private IEnumerator Client_Plugins_List_Update()
        {
            loading_tab.Select();
            ld_text.Text = "Fetching master plugins list";
            retry_pl_list_btn.isVisible = false;
            yield return new WaitForSeconds(0.4f);

            bool b = true, fail = false;
            IEnumerator iter = null;
            try
            {
                iter = Git_Updater.instance.Cache_And_Open_File_Async(PLUGINS_LIST_URL);
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
                fail = true;
            }
            //while (iter.MoveNext()) yield return null;
            while(b && !fail)
            {
                try
                {
                    if (iter == null)
                    {
                        fail = true;
                        break;
                    }
                    b = iter.MoveNext();
                }
                catch (Exception ex)
                {
                    SLog.Error(ex);
                    fail = true;
                }

                //if(b) yield return null;// wait
                yield return null;// wait
            }


            if (fail == false)
            {
                ld_text.Text = "Listing plugins";
                if (iter.Current != null)
                {
                    try
                    {
                        FileStream strm = iter.Current as FileStream;

                        byte[] data = Util.Read_Stream(strm);
                        var list = JSON.Parse(Encoding.ASCII.GetString(data))["plugins"];
                        plugins.Clear();

                        foreach (JSONNode json in list.Childs)
                        {
                            var dat = new Plugin_Download_Data(json);
                            plugins[dat.Hash] = dat;
                        }

                        lbl_pl_count.Value = plugins.Count.ToString();
                        pending_rebuild = true;
                        plugins_tab.Select();
                    }
                    catch (Exception ex)
                    {
                        SLog.Info("Error while updating plugins list.");
                        SLog.Error(ex);
                        fail = true;
                    }
                }
                else fail = true;
            }

            if (fail)
            {
                retry_pl_list_btn.isVisible = true;// show the retry button
                ld_text.Text = "unable to load plugin master list";
                yield break;// exit
            }
            else
            {
                retry_pl_list_btn.isVisible = false;// show the retry button
                ld_text.Text = "Done!";
                plugins_tab.Select();
            }

            yield break;
        }

        private void Rebuild_Plugins_UI()
        {
            pending_rebuild = false;
            list.Clear_Children();
            string search_str = search.text.ToLower();

            // add all the plugins to the sidebar
            foreach (KeyValuePair<string, Plugin_Download_Data> kv in this.plugins)
            {
                Plugin_Download_Data plugin = kv.Value;

                Plugin_Data dat = new Plugin_Data()
                {
                    AUTHOR = plugin.Author,
                    NAME = plugin.Name,
                    DESCRIPTION = plugin.Description
                };
                
                if (search_str.Length > 0)
                {
                    bool match = false;
                    if (plugin.Name.ToLower().IndexOf(search_str) > -1) match = true;
                    if (plugin.Author.ToLower().IndexOf(search_str) > -1) match = true;

                    if (!match) continue;
                }
                              

                Plugin_StoreItem itm = uiControl.Create<Plugin_StoreItem>();
                itm.onClicked += this.Select_Plugin;
                itm.Set_Plugin_Data(dat);
                list.Add(itm.plugin_hash, itm);
            }

            // select one of the plugins 
            if (list.Get_Children().Count() > 0)
            {
                if (this.selected_plugin == null)
                {
                    //Select nothing, show instructions instead
                    //Select_Plugin(list.Get_Children()[0]);
                    tabPanel.Select_Tab(INSTRUCTION_TAB_NAME);
                }
                else
                {
                    uiControl found = null;
                    foreach (uiControl c in this.list.Get_Children())
                    {
                        Plugin_StoreItem itm = (Plugin_StoreItem)c;
                        if (itm.plugin_hash == selected_plugin.plugin_hash)
                        {
                            found = c;
                            break;
                        }
                    }

                    if (found != null) Select_Plugin(found);
                    else Select_Plugin(list.Get_Children().First());
                }
            }
            else Select_Plugin(null);

        }

        private void Download_Plugin(string hash, string url)
        {
            Plugin_Download_Data plData = plugins[hash];
            string pl_title = "unknown";
            if (plData != null) pl_title = plData.Title;

            if(plData.Updater == null)
            {
                SLog.Info("The plugin \"{0}\" has an invalid updater instance, it's update method might not have been specified by the author!", pl_title);
                return;
            }

            Plugin_StoreItem plugin = (list[hash] as Plugin_StoreItem);
            plugin.download_info.Expand();
            //plugin.download_info.Collapse();
            plugin.progress_text.Text = "Connecting...";
            plugin.progress_text.Value = "0%";
            
            string local_file = String.Format("{0}\\..\\plugins\\{1}", UnityEngine.Application.dataPath, plData.Filename);
            StartCoroutine(plData.Updater.DownloadAsync(url, local_file, (string ContentType) =>
           {
               (list[hash] as Plugin_StoreItem).progress_text.Text = "Downloading:";
               if (ContentType.StartsWith("application/")) return true;//yea it's binary file data

               SLog.Info("The download url for the plugin \"{0}\" returns content of type \"{1}\" rather than the plugin file itself.\nThis may indicate that the url for this plugin leads to a download PAGE as opposed to giving the actual file, the plugin creator should supply a valid url leading DIRECTLY to the file.", pl_title, ContentType);
               return false;//This file is not ok to download.
           },
           (float read, float total) =>
           {
               if (list == null) return;

               Plugin_StoreItem pl = list[hash] as Plugin_StoreItem;
               if (pl != null)
               {
                   float pct = ((float)read / (float)total);
                   pl.progress_bar.Value = pct;
                   pl.progress_text.Value = pct.ToString("P0");
               }
            },
           (string filename) =>
           {
               bool success = Loader.Add_Plugin_To_List(filename);
               if (success)
               {
                   Plugin pl = Plugin.Get_Plugin(plData.Hash);
                   if (pl != null) pl.Enable();//enable the plugin by default, since they JUST installed it and all.
                   else throw new Exception("Cannot find and enable plugin: "+ plData.Name);

                   Select_Plugin(selected_plugin);
               }
           }));
        }
    }
}
