using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    class Plugin_Update_Viewer : uiWindow
    {
        private bool first_show = true;
        private uiScrollPanel list = null;
        private uiCheckbox mark_all = null;
        private uiButton btn_update = null;
        private uiTextArea msg_instruct = null;
        private List<Plugin> download_queue = new List<Plugin>();


        public Plugin_Update_Viewer()
        {
            onLayout += Plugin_Update_Viewer_onLayout;
            Title = "Updates Available";
            Set_Size(400, 500);
            CenterVertically();
            alignLeftSide(5f);
            onShown += Plugin_Update_Viewer_onShown;


            list = Create<uiScrollPanel>(this);
            list.Margin = new RectOffset(5, 5, 0, 5);
            list.Padding = new RectOffset(2, 2, 2, 2);
            list.onLayout += List_onLayout;
            list.Border.normal.color = Color.grey;

            mark_all = Create<uiCheckbox>(this);
            mark_all.text = "Mark/Unmark All";
            mark_all.isChecked = true;
            mark_all.onChange += Mark_all_onChange;

            btn_update = Create<uiButton>(this);
            btn_update.Text = "Update";
            btn_update.local_style.fontSize = 18;
            btn_update.onClicked += Btn_start_onClicked;

            msg_instruct = Create<uiTextArea>(this);
            msg_instruct.Margin = new RectOffset(3, 3, 3, 3);
            msg_instruct.Text = "<b>Some of your plugins have updates available</b>\n<i><color=#BBBBBB>Select plugins to update then click the button at the bottom to begin!</color></i>";
            
        }

        private void Plugin_Update_Viewer_onLayout(uiPanel c)
        {
            msg_instruct.alignTop(5f);
            msg_instruct.alignLeftSide();
            msg_instruct.FloodX();

            mark_all.alignLeftSide(5f);
            mark_all.moveBelow(msg_instruct, 12f);

            list.moveBelow(mark_all, 5f);
            list.FloodXY(0f, 32f);

            btn_update.moveBelow(list, 2f);
            btn_update.CenterHorizontally();
        }

        private void Btn_start_onClicked(uiControl c)
        {
            foreach (uiControl child in list.Get_Children())
            {
                Plugin_Update_Item itm = child as Plugin_Update_Item;
                if(itm.isChecked)
                {
                    Plugin plugin = Plugin.Get_Plugin(itm.plugin_hash);
                    download_queue.Add(plugin);
                }
            }

            process_downloads();
        }

        private void process_downloads()
        {
            if (download_queue.Count > 0)
            {
                Plugin plugin = download_queue.First();
                Plugin_Update_Item itm = list.Get_Children().First(o => ((Plugin_Update_Item)o).plugin_hash == plugin.Hash) as Plugin_Update_Item;
                if(itm == null)
                {
                    SLog.Info("Unable to find plugin_update_item for plugin {0}({1})", plugin.data.NAME, plugin.Hash);
                    return;
                }
                else
                {
                    SLog.Info("Found plugin_update_item for plugin {0}({1})", plugin.data.NAME, plugin.Hash);
                }

                uiProgressBar prog = null;
                if (itm != null) prog = itm.progress_bar;

                StartCoroutine( plugin.force_download(prog, (string file) =>
                {
                    download_queue.RemoveAt(0);
                    process_downloads();
                }));
            }
            else
            {
                this.Hide();
                Loader.Restart_App();
            }
        }

        private void Mark_all_onChange(uiCheckbox c, bool was_clicked)
        {
            foreach (uiControl child in list.Get_Children())
            {
                Plugin_Update_Item itm = child as Plugin_Update_Item;
                itm.isChecked = c.isChecked;
            }
        }

        private void List_onLayout(uiControl c)
        {
            // Arrange all children into a cascading list
            Plugin_Update_Item last = null;
            foreach (uiControl child in list.Get_Children())
            {
                Plugin_Update_Item itm = child as Plugin_Update_Item;
                if (last == null) itm.alignTop();
                else itm.moveBelow(last);

                itm.FloodX();
                itm.isChecked = true;
                last = itm;
            }
        }
        
        private void Plugin_Update_Viewer_onShown(uiWindow c)
        {
            if (!this.first_show) return;
            this.first_show = false;

            foreach (Plugin plugin in Loader.plugins.Values)
            {
                bool has_update = plugin.check_for_updates();
                if(has_update)
                {
                    var itm = Create<Plugin_Update_Item>(list);
                    itm.Set_Plugin_Data(plugin.data);
                }
            }
        }
        
        protected void Start()
        {
            StartCoroutine(this.CheckForUpdates());
        }

        private IEnumerator CheckForUpdates()
        {
            //PLog.Info("Checking for plugin updates...");
            int updates = 0;
            foreach (Plugin plugin in Loader.plugins.Values)
            {
                try
                {
                    bool has_update = plugin.check_for_updates();
                    if (has_update) updates++;
                    //PLog.Info("Plugin[{0}] has_update = {1}", plugin.data.NAME, (has_update ? "TRUE" : "FALSE"));
                }
                catch(Exception ex)
                {
                    SLog.Error(ex);
                }
            }

            if (updates <= 0) this.Hide();
            else this.Show();

            yield break;
        }
    }
}
