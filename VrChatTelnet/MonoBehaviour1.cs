using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using VrChatTelnet;

[assembly: MelonInfo(typeof(MonoBehaviour1), "VrChatTelent", "1.0", "noah")]
[assembly: MelonGame(null, null)]
namespace VrChatTelnet
{
    public class MonoBehaviour1 : MelonMod
    {
        public static ClientInstance myinstance;
        public static IrcConsoleGui mygui;
        public override void OnApplicationStart()
        {
            myinstance = new ClientInstance();
            ClassInjector.RegisterTypeInIl2Cpp<IrcConsoleGui>();
            base.OnApplicationStart();
        }
        public override void OnLevelWasLoaded(int level)
        {
            var myobject = new GameObject().TryCast<GameObject>();
            mygui = myobject.AddComponent<IrcConsoleGui>();
        }

        public void VtCoreStuff()
        {
        }
    }
}