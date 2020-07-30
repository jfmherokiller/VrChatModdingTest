using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// Manages saving & loading a sequence of config key/value pairs to a file.
    /// </summary>
    public class SettingsFile
    {
        /// <summary>
        /// A delegate that gets called in the event that the specified settings file does not exhist.
        /// </summary>
        private Action<string> onCreateFile;
        private string FILE = null;
        private JSONClass json = null;

        /// <summary>
        /// Please note that with the <see cref="SimpleJSON"/> system key-value associations are actually instances of <see cref="JSONClass"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JSONNode this[string key] { get { if (json == null) { throw new Exception("JSONClass is not ready!"); } if (!json.ContainsKey(key)) { return null; } return json[key]; } set { json[key] = value; } }


        /// <summary>
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="dir"></param>
        /// <param name="onCreateDelegate">A delegate that is called in the event that the specified settings file does not exist</param>
        public SettingsFile(string filename, string dir=null, Action<string> onCreateDelegate=null)
        {
            onCreateFile = onCreateDelegate;
            if (dir == null) dir = UnityEngine.Application.dataPath;
            FILE = Path.GetFullPath(Path.Combine(dir, filename));
            Load();
        }

        #region Saving & Loading

        /// <summary>
        /// Load the specified settings file, please note that the file is already loaded by the constructor so this function really RELOADS the file.
        /// </summary>
        public void Load()
        {
            json = new JSONClass();// Go ahead and create an empty instance just incase we can't actually load from the specified file.
            if (!File.Exists(FILE))
            {//it's ok we will create it!
                onCreateFile?.Invoke(FILE);
                var fs = File.CreateText(FILE);
                fs.Close();
                Save();
                return;
            }

            string str = File.ReadAllText(FILE);
            if (str == null || str.Length <= 0) return;
            
            json = (JSONClass)JSON.Parse(str);
        }

        public void Save()
        {
            string FILE_TMP = String.Concat(FILE, ".tmp");
            FileStream strm = File.Open(FILE_TMP, FileMode.OpenOrCreate);
            byte[] buf = Encoding.UTF8.GetBytes(json.ToString(""));
            strm.BeginWrite(buf, 0, buf.Length, (IAsyncResult result) => {
                strm.EndWrite(result);
                strm.Close();
                File.Copy(FILE_TMP, FILE, true);
                File.Delete(FILE_TMP);
            }, null);
        }

        #endregion

        #region Setters

        public void Set_Array<T>(string key, IEnumerable<T> list)
        {
            JSONArray dat = new JSONArray();
            foreach(T o in list) dat.Add(new JSONData(o.ToString()));
            
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }

        public void Set_Bool(string key, bool v)
        {
            JSONData dat = new JSONData(v);
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }

        public void Set_Int(string key, int v)
        {
            JSONData dat = new JSONData(v);
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }

        public void Set_Float(string key, float v)
        {
            JSONData dat = new JSONData(v);
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }

        public void Set_Double(string key, double v)
        {
            JSONData dat = new JSONData(v);
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }

        public void Set_String(string key, string v)
        {
            JSONData dat = new JSONData(v);
            if (!json.ContainsKey(key)) json.Add(key, dat);
            else json[key] = dat;
        }
        #endregion

        #region Getters

        public List<T> Get_Array<T>(string key)
        {
            var ty = typeof(T);
            if (!ty.IsOneOf(new Type[]{ typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double) })) throw new ArgumentException("Type \""+typeof(T).Name+"\" is not a supported type. Supported types are: string, bool, int, float, and double!");

            List<object> list = new List<object>();
            if (json.ContainsKey(key))
            {
                JSONArray arr = json[key].AsArray;
                foreach (JSONData o in arr)
                {
                    if (typeof(string) == ty) list.Add(o.Value);
                    else if (typeof(int) == ty) list.Add(o.AsInt);
                    else if (typeof(bool) == ty) list.Add(o.AsBool);
                    else if (typeof(float) == ty) list.Add(o.AsFloat);
                    else if (typeof(double) == ty) list.Add(o.AsDouble);
                }
            }
            
            return list.Cast<T>().ToList<T>();
        }

        public bool Get_Bool(string key) { return json[key].AsBool; }

        public int Get_Int(string key) { return json[key].AsInt; }

        public long Get_Long(string key)
        {
            long v = 0;
            if (long.TryParse(json[key].Value, out v)) return v;
            return 0;
        }

        public ulong Get_ULong(string key)
        {
            ulong v = 0;
            if (ulong.TryParse(json[key].Value, out v)) return v;
            return 0;
        }

        public float Get_Float(string key) { return json[key].AsFloat; }

        public double Get_Double(string key) { return json[key].AsDouble; }

        public string Get_String(string key) { return json[key].Value; }
        #endregion
    }
}
