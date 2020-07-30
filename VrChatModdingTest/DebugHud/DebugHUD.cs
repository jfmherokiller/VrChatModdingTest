using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public static class DebugHud
    {
        private static GameObject hud_root = null;
        private static DebugHUD_Renderer hud = null;

        private static List<string> lines = new List<string>();
        private static Dictionary<string, int> stacks = new Dictionary<string, int>();
        private static FileStream log_file = null;
        private static string[] html_tags = null;


        public static void Init()
        {
            if (DebugHud.hud_root == null)
            {
                DebugHud.hud_root = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(DebugHud.hud_root);
            }

            if (DebugHud.hud == null)
            {
                DebugHud.hud = DebugHud.hud_root.AddComponent<DebugHUD_Renderer>();
                UnityEngine.Object.DontDestroyOnLoad(DebugHud.hud);
            }

            Logging.Logger.onLog += Logger_onLog;

            
            string[] tags = new string[] { "b", "size", "color" };
            List<string> tmp = new List<string>();

            foreach(string tag in tags)
            {
                tmp.Add(String.Format("<{0}>", tag));
                tmp.Add(String.Format("</{0}>", tag));
            }

            html_tags = tmp.ToArray();
        }

        private static void Logger_onLog(Logging.LogLevel level, string module, string msg)
        {
            if (DebugHud.hud == null)
            {
                DebugHud.lines.Add(msg);
            }
            else
            {
                if (DebugHud.lines.Count > 0)
                {
                    foreach (string s in DebugHud.lines)
                    {
                        DebugHud.hud.Add_Line(s);
                    }
                    DebugHud.lines.Clear();
                }
                DebugHud.hud.Add_Line(msg);
            }
        }

        [Obsolete("The DebugHud class is obsolete, use the Log class instead!", true)]
        public static void Log(string format)
        {
            string str = DebugHud.Tag_String(format, 1);
            DebugHud.Add_Line(str);
        }

        [Obsolete("The DebugHud class is obsolete, use the Log class instead!", true)]
        public static void Log(string format, params object[] args)
        {
            string str = DebugHud.Tag_String(String.Format(format, args), 1);
            DebugHud.Add_Line(str);
        }

        [Obsolete("The DebugHud class is obsolete, use the Log class instead!", true)]
        public static void Log(Exception ex)
        {
            string str = DebugHud.Format_Exception_Log(ex, 1);
            DebugHud.Add_Line(str, true);
        }

        [Obsolete("The DebugHud class is obsolete, use the Log class instead!", true)]
        public static void Log(GameObject obj)
        {
            Log("GameObject<{0}>  {1}", obj.GetInstanceID(), GameObject_Components_ToString(obj));
        }



        [Obsolete("The DebugHud class is obsolete, use the Log.Debug() to silently log messages!", true)]
        public static void LogSilent(string format, params object[] args)
        {
            string str = DebugHud.Format_Log(format, 1, args);
            DebugHud.write_log(str);
        }

        [Obsolete("The DebugHud class is obsolete, use the Log.Debug() to silently log messages!", true)]
        public static void LogSilent(string str)
        {
            DebugHud.write_log(str);
        }

        [Obsolete("The DebugHud class is obsolete, use the Log.Debug() to silently log messages!", true)]
        public static void LogSilent(Exception ex)
        {
            string str = DebugHud.Format_Exception_Log(ex, 0);
            DebugHud.write_log(str);
        }

        private static string strip_html_tags(string str)
        {
            foreach(string tag in html_tags)
            {
                str = str.Replace(tag, "");
            }
            return str;
        }
        
        private static void open_log_stream()
        {
            /*
            if (DebugHud.log_file != null) DebugHud.log_file.Close();

            string logPath = String.Format("{0}/Plugins.log", UnityEngine.Application.dataPath);
            DebugHud.log_file = new FileStream(logPath, FileMode.Create);
            */
        }

        private static void write_log(string str, bool write_to_unity=false)
        {
            //if (DebugHud.log_file == null) DebugHud.open_log_stream();

            if (!str.EndsWith("\n")) str += "\n";
            str = Logging.Logger.strip_html_tags(str);
            if(write_to_unity) UnityEngine.Debug.Log(String.Format("[DebugHUD] {0}", str));

            /*

            DebugHud.log_file.Write(bytes, 0, bytes.Length);
            DebugHud.log_file.Flush();
            */
        }

        private static void write_log(string format, params object[] args)
        {
            string str = String.Format(format, args);
            write_log(str);
        }

        public static string Format_Exception_Log(Exception ex, int stack_offset = 0)
        {
            string str = null;
            try
            {
                //string trace = new StackTrace(ex, 1 + stack_offset, true).ToString();
                string trace = StackTraceUtility.ExtractStringFromException(ex);
                str = String.Format("{0}\n{1}", ex.Message, trace);
            }
            catch(Exception e)
            {
                write_log("DebugHUD.Format_Exception_Log() THREW AN INTERNAL EXCEPTION!\n{0}\n{1}", e.Message, e.StackTrace);
                str = String.Format("{0}\n{1}", ex.Message, StackTraceUtility.ExtractStackTrace());
            }

            if (ex.InnerException != null) str = String.Format("{0}\n{1}", str, ex.InnerException.Message);

            return DebugHud.Format_Log(str, 2+stack_offset);//reformat our exception string with an additional stack offset of 1 to make it skip past the function that called THIS format function.
            //return DebugHud.Tag_String(str, stack_offset);
        }

        public static string Tag_String(string str, int stack_offset=0)
        {
            int idx = 0;
            string class_name = null;
            StackFrame frame = null;

            // Search upwards through the callstack to find the first classname that isnt DebugHud. USUALLY this will only loop once and cost us no more time then the old method I used to use. 
            // But this one will catch those odd cases where this function gets called from a big hierarchy of DebugHud functions and STILL give us the plugin name we so want!
            while(class_name==null || String.Compare("DebugHud", class_name)==0)
            {
                // pre incrementing makes idx = 1 on the first loop
                frame = new StackFrame(++idx + stack_offset, false);
                class_name = frame.GetMethod().DeclaringType.Name;
            }

            string plugin_name = frame.GetMethod().Module.ScopeName;
            return String.Format("<b>{0}</b>  {1}", plugin_name, str);
        }

        public static string Format_Log(string format, int stack_offset = 0, params object[] args)
        {
            StackFrame frame = new StackFrame(1 + stack_offset, true);
            string meth = frame.GetMethod().ReflectedType.FullName;
            string funct = frame.GetMethod().Name;
            int line = frame.GetFileLineNumber();

            string str = String.Format(format, args);
            str = DebugHud.Tag_String(String.Format("{0}  [Function: {1}::{2} @ Line: {3}]", str, meth, funct, line), 1);

            return str;
        }

        public static string AppendTimeStamp(string line)
        {
            return String.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), line);
        }

        private static void Add_Line(string str, bool write_to_unity=false)
        {
            /*
            str = DebugHud.AppendTimeStamp(str);
            DebugHud.write_log(str, write_to_unity);

            if (DebugHud.hud == null)
            {
                DebugHud.lines.Add(str);
            }
            else
            {
                if (DebugHud.lines.Count > 0)
                {
                    foreach (string s in DebugHud.lines)
                    {
                        DebugHud.hud.Add_Line(s);
                    }
                    DebugHud.lines.Clear();
                }
                DebugHud.hud.Add_Line(str);
            }
            */
        }

        public static string GameObject_Components_ToString(GameObject obj)
        {
            Component[] comps = obj.GetComponents<Component>();
            string str = "";
            foreach (var c in comps) str = String.Format("{0}, {1}", str, c.GetType());
            return str;
        }
        

        public static void Reset()
        {
            if (DebugHud.hud != null) DebugHud.hud.Clear();
        }

    }
}
