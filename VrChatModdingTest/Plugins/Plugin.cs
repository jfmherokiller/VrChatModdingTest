using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Security.Policy;
using System.Security;
using System.Runtime.InteropServices;

namespace SR_PluginLoader
{
    public partial class Plugin
    {
        #region Variables

        public Plugin_Data data = null;
        public string Hash { get { if (data==null) { throw new ArgumentNullException("Plugin DATA is NULL!"); } return data.Hash; } }
        protected Updater_Base Updater { get { return Updater_Base.Get_Instance(data.UPDATE_METHOD.METHOD); } }
        public override string ToString() { return String.Format("Plugin[{0}.{1}]", data.NAME, data.AUTHOR); }


        /// <summary>
        /// Gets the SHA1 hash for the currently installed version of the plugin so it can be compared to other plugin dll's
        /// </summary>
        private string _cached_data_hash = null;
        public string Data_Hash {
            get
            {
                if (!File.Exists(FilePath)) return null;
                if (_cached_data_hash == null) _cached_data_hash = Util.Git_File_Sha1_Hash(FilePath);

                return _cached_data_hash;
            }
        }

        /// <summary>
        /// The game object assigned to manage this plugin.
        /// </summary>
        private GameObject GM = null;
        private string Unique_GameObject_Name { get { return this.ToString(); } }

        private bool first_load = true;
        private bool is_update_available = false;
        public bool Enabled = false;
        /// <summary>
        /// Does this plugin have dependencys that arent currently met?
        /// </summary>
        public bool has_dependency_issues = false;
        public List<Plugin_Dependency> unmet_dependencys = new List<Plugin_Dependency>();
        
        public string FilePath = null;
        private DateTime? cached_file_time = null;
        public DateTime file_time { get { if (!cached_file_time.HasValue) { cached_file_time = File.GetCreationTime(FilePath); } return cached_file_time.Value; } }
        public string FileDir = null;
        private string dll_name = null;

        public List<string> Errors = new List<string>();
        public bool HasErrors { get { return (Errors.Count > 0); } }
        public Action onError;

        /// <summary>
        /// Returns True/False whether or not this plugin has the ability to auto update.
        /// </summary>
        public bool CanAutoUpdate { get { return (data!=null && data.UPDATE_METHOD != null && data.UPDATE_METHOD.Valid); } }

        private FILE_UPDATE_STATUS _update_status = FILE_UPDATE_STATUS.UP_TO_DATE;//Assume up to date by default to avoid glitches if this plugin CANT update or something.
        public FILE_UPDATE_STATUS UpdateStatus { get { return _update_status; } }


        public Texture2D icon = null;
        public Texture2D thumbnail = null;
        
        private Assembly dll = null;
        private Type pluginClass = null;
        private MethodInfo load_funct = null;
        private MethodInfo unload_funct = null;
        #endregion


        public Plugin(string file, bool en = false)
        {
            FilePath = file;
            FileDir = Path.GetDirectoryName(file);
            dll_name = Path.GetFileName(file);

            Enabled = en;
        }


        /// <summary>
        /// This is where we actually do all the loading, to prevent any exceptions from causing the plugin instance to not be put into our global plugins map.
        /// This way we can ensure any errors that we DO get while loading can be properly displayed at the plugins menu!
        /// </summary>
        public void Setup()
        {
            Load_DLL();
            Load_Plugin_Info();
            Load_Assets();
        }
        
        private void Load_Assets()
        {
            if (this.dll == null) return;
            string icon_file = this.data.ICON;
            string thumb_file = this.data.PREVIEW;

            
            if (icon_file != null)
            {
                byte[] buf = this.Load_Resource(icon_file);
                if (buf != null)
                {
                    this.icon = (Texture2D)TextureHelper.Load(buf, icon_file);
                }
                else
                {
                    this.icon = null;
                }
            }

            if (thumb_file != null)
            {
                byte[] buf = this.Load_Resource(thumb_file);
                if (buf != null)
                {
                    this.thumbnail = (Texture2D)TextureHelper.Load(buf, thumb_file);
                }
                else
                {
                    this.thumbnail = null;
                }
            }
        }

        private void Load_Plugin_Info()
        {
            try
            {
                var field = pluginClass.GetField("PLUGIN_INFO", BindingFlags.Public | BindingFlags.Static);
                if (field == null) return;

                data = (Plugin_Data)field.GetValue(null);
                data.DESCRIPTION = data.DESCRIPTION.Trim(new char[] {'\n', '\r' });
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
            }
        }
        

        private void Add_Error(string format, params object[] args)
        {
            string str = DebugHud.Format_Log(format, 1, args);
            Errors.Add(str);
            SLog.Info("[ <b>{0}</b> ] {1}", this.dll_name, str);
            onError?.Invoke();
        }

        private void Add_Error(Exception ex)
        {
            string str = Logging.Logger.Format_Exception(ex);
            Errors.Add(str);
            SLog.Error("[ <b>{0}</b> ] {1}", dll_name, str);
            onError?.Invoke();
        }

        private bool Load_DLL()
        {
            try
            {
                dll = load_assembly(dll != null);

                //find the static SR_Plugin class amongst however many namespaces this library has.
                foreach (Type ty in dll.GetExportedTypes())
                {
                    if (String.Compare(ty.Name, "SR_Plugin")==0)
                    {
                        if (ty.IsPublic == false) continue;
                        pluginClass = ty;
                        break;
                    }
                }

                if (pluginClass == null)
                {
                    Add_Error("Unable to locate a static 'SR_Plugin' class in the loaded library.");
                    return false;
                }


                load_funct = pluginClass.GetMethod("Load", BindingFlags.Static | BindingFlags.Public);
                unload_funct = pluginClass.GetMethod("Unload", BindingFlags.Static | BindingFlags.Public);

                if (load_funct == null) Add_Error("Unable to find Load function!");
                if (unload_funct == null) Add_Error("Unable to find Unload function!");



                if (load_funct == null || unload_funct == null)
                {
                    SLog.Info("Unable to locate load/unload functions.");
                    return false;
                }

                //PLog.Info("[{0}] Plugin loaded!", this.dll_name);
                return true;
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
                return false;
            }
        }
        

        private Assembly load_assembly(bool reload=false)
        {
            try
            {
                string FN_NoExt = Path.GetFileNameWithoutExtension(FilePath);
                string pdb_file = String.Concat(FileDir, "\\", FN_NoExt, ".pdb");

                if (reload == true)
                {
                }
                
                

                byte[] dll_buf = Load_Bytes(FilePath);
                byte[] pdb_buf = null;
                if(File.Exists(pdb_file)) Load_Bytes(pdb_file);

                if (dll_buf == null) return null;

                Assembly asm;
                /*
                if (pdb_buf != null) asm = Assembly.Load(dll_buf, pdb_buf);
                else asm = Assembly.Load(dll_buf);
                */

                if (pdb_buf != null) asm = AppDomain.CurrentDomain.Load(dll_buf, pdb_buf);
                else asm = AppDomain.CurrentDomain.Load(dll_buf);

                return asm;
            }
            catch(Exception ex)
            {
                Add_Error(ex);
            }

            return null;
        }

        private byte[] Load_Bytes(string file)
        {
            try
            {
                using (FileStream stream = new FileStream(file, FileMode.Open))
                {
                    byte[] buf = new byte[stream.Length];
                    int read = stream.Read(buf, 0, (int)stream.Length);
                    if (read < (int)stream.Length)
                    {
                        int remain = ((int)stream.Length - read);
                        int r = 0;
                        while (r < remain && remain > 0)
                        {
                            r = stream.Read(buf, read, remain);
                            read += r;
                            remain -= r;
                        }
                    }

                    return buf;
                }
            }
            catch (Exception ex)
            {
                this.Add_Error(ex);
            }

            return null;
        }

        public void Process_Dependencys()
        {
            if (this.data.DEPENDENCIES.Count <= 0) return;
            this.unmet_dependencys.Clear();
            Dictionary<Plugin_Dependency, PLUGIN_DEP_COMPARISON_FLAG> met_depends = new Dictionary<Plugin_Dependency, PLUGIN_DEP_COMPARISON_FLAG>();
            foreach(var dep in data.DEPENDENCIES)
            {
                met_depends[dep] = PLUGIN_DEP_COMPARISON_FLAG.FALSE;
            }

            foreach(KeyValuePair<string, Plugin> kv in Loader.plugins)
            {
                if (kv.Value == this) continue;
                foreach (Plugin_Dependency dep in data.DEPENDENCIES)
                {
                    var cmp_res = dep.Compare(kv.Value);
                    if (cmp_res != PLUGIN_DEP_COMPARISON_FLAG.FALSE)
                    {
                        met_depends[dep] = cmp_res;
                        break;//go to next plugin
                    }
                }
            }

            foreach(KeyValuePair<Plugin_Dependency, PLUGIN_DEP_COMPARISON_FLAG> kv in met_depends)
            {
                if (kv.Value != PLUGIN_DEP_COMPARISON_FLAG.SAME) unmet_dependencys.Add(kv.Key);
            }
        }

        public bool Enable()
        {
            Enabled = false;//default it and if we successfully load THEN we can make it true.
            if (has_dependency_issues == true)
            {
                Enabled = false;
                return false;
            }

            if ( Load() )
            {
                Enabled = true;
                bool was_first = first_load;
                first_load = false;
                if (!was_first) Loader.Plugin_Status_Change(this, Enabled);
            }

            return Enabled;
        }

        private bool Load()
        {
            var dupe = GameObject.Find(Unique_GameObject_Name);
            if (dupe != null)
            {
                GameObject.DestroyImmediate(dupe);
                SLog.Info("Destroyed pre-existing plugin manager GameObject instance!");
            }

            GameObject gmObj = new GameObject(Unique_GameObject_Name);
            UnityEngine.GameObject.DontDestroyOnLoad(gmObj);

            try
            {
                if (load_funct != null)
                {
                    object[] args = new object[load_funct.GetParameters().Length];
                    var paramz = load_funct.GetParameters();
                    for (int i = 0; i < paramz.Length; i++)
                    {
                        ParameterInfo param = paramz[i];
                        if (typeof(GameObject) == param.ParameterType) args[i] = gmObj;
                        else if (typeof(Plugin) == param.ParameterType) args[i] = this;

                    }

                    load_funct.Invoke(null, args);
                }
                else return false;

                GM = gmObj;
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
                Add_Error(ex);
                Unload();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to execute the plugins unload logic.
        /// </summary>
        private void Unload()
        {
            try
            {
                if (unload_funct != null)
                {
                    object[] args = new object[unload_funct.GetParameters().Length];
                    var paramz = unload_funct.GetParameters();
                    for (int i = 0; i < paramz.Length; i++)
                    {
                        ParameterInfo param = paramz[i];
                        if (typeof(GameObject) == param.ParameterType) args[i] = GM;
                        else if (typeof(Plugin) == param.ParameterType) args[i] = this;
                    }

                    this.unload_funct.Invoke(null, args);
                }
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
                Add_Error(ex);
            }
            finally
            {
                if (GM != null) UnityEngine.GameObject.Destroy(GM);
                GM = null;
                Loader.Plugin_Status_Change(this, Enabled);
            }
        }

        /// <summary>
        /// unloads the plugin and sets Enabled to false
        /// </summary>
        public void Disable()
        {
            Enabled = false;// Unloading doesn't follow the same rules as loading, we just assume it's unloaded for the user's sake.
            Unload();
        }

        public void Toggle()
        {
            if (this.Enabled == true)
            {
                this.Disable();
            }
            else
            {
                this.Enable();
            }
        }

        public void Uninstall()
        {
            Disable();

            string name = Path.GetFileNameWithoutExtension(FilePath);
            Loader.plugins.Remove(name);
            File.Delete(FilePath);
        }
        
        public byte[] Load_Resource(string name)
        {
            try
            {
                string[] assets = this.dll.GetManifestResourceNames();
                if (assets == null || assets.Length <= 0) return null;
                
                string asset_name = assets.FirstOrDefault(r => r.EndsWith(name));
                if (asset_name == null || asset_name.Length <= 0) return null;


                using (Stream stream = this.dll.GetManifestResourceStream(asset_name))
                {
                    if (stream == null) return null;

                    byte[] buf = new byte[stream.Length];
                    int read = stream.Read(buf, 0, (int)stream.Length);
                    if (read < (int)stream.Length)
                    {
                        int remain = ((int)stream.Length - read);
                        int r = 0;
                        while (r < remain && remain > 0)
                        {
                            r = stream.Read(buf, read, remain);
                            read += r;
                            remain -= r;
                        }
                    }

                    return buf;
                }
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Causes this plugin to check and see if it has any updates.
        /// </summary>
        public bool check_for_updates()
        {
            if (is_update_available) return true;

            if(data==null)
            {
                SLog.Warn("{0} Plugin has no DATA structure!", this);
                return false;
            }

            if (data.UPDATE_METHOD == null || !data.UPDATE_METHOD.Valid)
                return false;

            _update_status = Updater.Get_Update_Status(data.UPDATE_METHOD.URL, FilePath);
            is_update_available = (_update_status == FILE_UPDATE_STATUS.OUT_OF_DATE);

            //PLog.Info("{0}  update_status: {1}", this, Enum.GetName(typeof(FILE_UPDATE_STATUS), status));
            return is_update_available;
        }

        /// <summary>
        /// Starts downloading the latest version of this plugin.
        /// (Coroutine)
        /// </summary>
        /// <param name="prog">uiProgressBar instance which this function will use to display the current update progress.</param>
        /// <returns></returns>
        public IEnumerator download_update(uiProgressBar prog, Updater_File_Download_Completed download_complete_cb)
        {
            if (!this.check_for_updates())
            {
                SLog.Info("{0} Plugin.download_update():  Already up to date!", this);
                yield break;
            }

            yield return this.force_download(prog, download_complete_cb);
        }

        /// <summary>
        /// Forces the plugin to download the latest version of itself.
        /// (Coroutine)
        /// </summary>
        /// <param name="prog">uiProgressBar instance which this function will use to display the current update progress.</param>
        /// <returns></returns>
        public IEnumerator force_download(uiProgressBar prog, Updater_File_Download_Completed download_complete_cb)
        {
            if (this.data.UPDATE_METHOD == null) throw new ArgumentNullException(String.Format("{0} Plugin has no UPDATE_METHOD specified!", this));
            
            IEnumerator iter = this.Updater.DownloadAsync(this.data.UPDATE_METHOD.URL, this.FilePath, null,
               (float current, float total) =>
               {//Download progress
                    float f = (float)current / (float)total;

                   if (prog != null)
                   {
                       float p = (float)current / (float)total;
                       prog.Value = p;
                   }
               },
               (string file) => 
               {
                   this.is_update_available = false;
                   if (download_complete_cb != null) download_complete_cb(file);
               });
            // Run the download coroutine within this coroutine without starting a seperate instance.
            while (iter.MoveNext()) yield return null;
            
            yield break;// Honestly probably not needed but I like to be safe because I still don't fully know the innerworkings of unity's coroutine system.
        }
    }
}
