using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public class Plugin_Data
    {
        public string AUTHOR = null;
        public string NAME = null;
        public string DESCRIPTION = null;
        public Plugin_Version VERSION = null;
        public List<Plugin_Dependency> DEPENDENCIES = new List<Plugin_Dependency>();
        public Update_Method UPDATE_METHOD = null;
        /// <summary>
        /// Name of the embedded image resource to use as this plugins icon.
        /// </summary>
        public string ICON = null;
        /// <summary>
        /// Name of the embedded image resource to use as this plugins preview image.
        /// </summary>
        public string PREVIEW = null;

        public Plugin_Data()
        {
        }

        public string Hash { get { return Util.SHA(String.Format("{0}.{1}", this.AUTHOR, this.NAME)); } }
        public static Plugin_Data FromJSON(SimpleJSON.JSONNode data)
        {
            return new Plugin_Data()
            {
                NAME = data["name"],
                AUTHOR = data["author"]
            };
        }

        public string ToJSON()
        {
            SimpleJSON.JSONClass js = new SimpleJSON.JSONClass();
            js["name"] = NAME;
            js["author"] = AUTHOR;
            js["description"] = DESCRIPTION;
            js["update_method"] = Enum.GetName(typeof(UPDATER_TYPE), UPDATE_METHOD.METHOD);
            js["url"] = UPDATE_METHOD.URL;
            
            return String.Concat("\"", NAME, ".", AUTHOR, "\"", ": ", js.ToString(), ",\n");
        }
    }
}
