using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiUpdatesAvailable : uiDialogResult
    {
        private uiListView list = null;
        public override uiControl this[string key] { get { return list[key]; } }
        private List<UpdateFile> file_list = new List<UpdateFile>();
        public List<UpdateFile> Files { get { return file_list; } }


        public uiUpdatesAvailable() : base()
        {
            Title = "Updates Available";
            Set_Size(300, 450);
            Center();

            contentPanel.onLayout += ContentPanel_onLayout;

            message.Text = "Updates are available!\nThe files which will be updated are listed below.";
            message.TextAlign = TextAnchor.UpperCenter;

            list = Create<uiListView>("list", contentPanel);
            //list.Set_Background(new UnityEngine.Color(0f, 0f, 0f, 0.2f));
            //list.Autosize_Method = AutosizeMethod.FILL;
            list.Autosize = false;
            //list.Set_Background(Color.clear);
            list.disableBG = true;
        }

        private void ContentPanel_onLayout(uiPanel c)
        {
            list.FloodXY();
        }

        public void Add_File(UpdateFile file)
        {
            file_list.Add(file);
            string filename = Path.GetFileName(file.FILE);

            var itm = Create<uiListItem_Progress>(filename, list);
            itm.Selectable = false;
            itm.Clickable = false;
            itm.Text = filename;
            itm.TextStyle = FontStyle.Bold;
            itm.TextSize = 14;
        }
    }


    public class UpdateFile
    {
        /// <summary>
        /// The full repo path for this file.
        /// </summary>
        public string FULLNAME;
        /// <summary>
        /// The relative file path.
        /// </summary>
        public string FILE;
        /// <summary>
        /// The blob download url for this file.
        /// </summary>
        public string URL;
        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public int SIZE;
        /// <summary>
        /// The local path where this file should be located.
        /// </summary>
        public string LOCAL_PATH = null;

        public UpdateFile(string _fullname, string _file, string _url, int byteCount)
        {
            FULLNAME = _fullname;
            FILE = _file;
            URL = _url;
            SIZE = byteCount;
        }
    }
}
