using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class PluginLoader_AutoUpdater : MonoBehaviour
    {
        public uiUpdatesAvailable updatesView = null;
        public List<UpdateFile> Files = new List<UpdateFile>();
        
        private void Start()
        {
            // Hide the dialogue window buttons while we are downloading so the user doesn't get confused and exit it.
            updatesView.Disable_Buttons();
            StartCoroutine(Start_Update());
        }   
        
        IEnumerator Start_Update()
        {
            foreach (UpdateFile file in Files)
            {
                byte[] buf = null;
                IEnumerator iter = Updater_Base.GetAsync(file.URL, null, (float read, float total) => {
                    if (updatesView != null)
                    {
                        var prog = (updatesView[file.FILE] as uiListItem_Progress);
                        if (prog != null) prog.Value = ((float)read / (float)total);
                    }
                });
                while (iter.MoveNext()) yield return null;

                if (iter.Current == null) continue;// go to the next file

                buf = iter.Current as byte[];
                if (buf == null || buf.Length <= 0) continue;// go to the next file

                string filename = file.LOCAL_PATH;// String.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(url));
                string new_file = String.Format("{0}.tmp", filename);
                string old_file = String.Format("{0}.old", filename);

                File.WriteAllBytes(new_file, buf);
                if (File.Exists(old_file)) File.Delete(old_file);
                File.Replace(new_file, filename, old_file);
            }
            // We have to restart the game for this to take effect.
            Loader.Restart_App();
            yield break;
        }
    }
}
