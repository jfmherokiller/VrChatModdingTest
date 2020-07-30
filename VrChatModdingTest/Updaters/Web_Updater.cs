using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;


namespace SR_PluginLoader
{
    public class Web_Updater : Updater_Base
    {
        private static readonly Web_Updater _instance = new Web_Updater();
        public static Web_Updater instance { get { return _instance; } }
        public static readonly UPDATER_TYPE type = UPDATER_TYPE.WEB;
        
    }
}
