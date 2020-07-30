using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class Loader_AsyncUpdatesCheck: MonoBehaviour
    {
        private void Awake() { UnityEngine.GameObject.DontDestroyOnLoad(base.gameObject); }

        private void Start() { StartCoroutine(Check_For_Updates()); }

        private IEnumerator Check_For_Updates()
        {
            SLog.Info("[AutoUpdater] Checking for updates...");
            List<UpdateFile> updates = new List<UpdateFile>();

            // We should automatically keep ALL files within the repositorys "installer" directory insync!
            var iter = Git_Updater.Get_Repo_Folder_Files_Async("https://raw.github.com/dsisco11/SR_Plugin_Loader/master/", "/Installer/");
            if(iter == null) { yield break; }
            while (iter.MoveNext()) yield return null;

            if (iter.Current != null)
            {
                List<UpdateFile> list = (List<UpdateFile>)iter.Current;
                string exDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (UpdateFile file in list)
                {
                    string FN = Path.GetFileName(file.FILE);
                    string dir = exDir;
                    if (String.Compare("SR_PluginLoader_Uninstaller.exe", FN) == 0) dir = Path.GetFullPath(String.Concat(dir, "/../../"));

                    string local_path = Path.Combine(dir, FN);
                    file.LOCAL_PATH = local_path;

                    var it = Git_Updater.instance.Get_Update_Status_Async(file.FULLNAME, local_path);
                    if (it == null) continue;
                    while (it.MoveNext()) yield return null;

                    FILE_UPDATE_STATUS status = (FILE_UPDATE_STATUS)it.Current;
                    //status = FILE_UPDATE_STATUS.OUT_OF_DATE;// DEBUG

                    //PLog.Info("{0}  |  LOCAL_PATH: {1}  |  REMOTE_PATH: {2}", file.FULLNAME, local_path, file.URL);
                    if (status == FILE_UPDATE_STATUS.OUT_OF_DATE) { updates.Add(file); }
                }
            }

            // If we have updates prompt the user to accept them.
            if (updates.Count > 0)
            {
                SLog.Info("[AutoUpdater] {0} Updates Available.", updates.Count);
                var updatesView = uiControl.Create<uiUpdatesAvailable>();
                updatesView.onResult += (DialogResult res) => {
                    if (res == DialogResult.OK)
                    {
                        // The user said OK so let's start downloading
                        var updater = (new GameObject().AddComponent<PluginLoader_AutoUpdater>());
                        updater.Files = updatesView.Files;
                        updater.updatesView = updatesView;
                        return true;
                    }
                    return false;
                };
                foreach (var f in updates) updatesView.Add_File(f);

                if (MainMenu.isReady) updatesView.Show();
                else SiscosHooks.Once(HOOK_ID.MainMenu_Loaded, () => { updatesView.Show(); });
            }
            else SLog.Info("[AutoUpdater] No updates.");

            yield return null;
            yield break;
        }
    }
}
