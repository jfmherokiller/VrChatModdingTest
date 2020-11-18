using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using RestrictionBypasser;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(MonoBehaviour1), "RestrictionKiller", "1.0", "Author Name")]
[assembly: MelonGame(null, null)]
namespace RestrictionBypasser
{

    public class MonoBehaviour1 : MelonMod
    {
        public static string settingsCategory = "CustomComponentBlockList";
        public static readonly string[] IrcMenuNames = {"IrcUserName", "IrcRealName","IrcNickName","IrcDefaultServer"};
        public static readonly string[] IrcMenuDisplay = {"Username to use for Irc","Real Name to use for Irc","Nick Name to use for Irc","Default server to use <servername>:<port>:<useSSL>"};
        public void RegisterModPrefs()
        {
            MelonPrefs.RegisterCategory(settingsCategory, "BlockList");
            MelonPrefs.RegisterString(settingsCategory,"CustomBlocker","");

        }
        public static IEnumerable<Type> GetTypes()
        {
            var blockedclasses = MelonPrefs.GetString(settingsCategory, "CustomBlocker").Split(';').Select(item =>
            {
                Type asss = null;
                try
                {
                    asss = Type.GetType(item);
                }
                catch (Exception e)
                {
                }

                return asss;
            });
            return blockedclasses;
        }
        public override void OnApplicationStart()
        {
            RegisterModPrefs();
            //FirstPatchSet();
           // SndPatchSet();
            Patcher3();
        }

        public void Patcher3()
        {
           var realPtr = UnhollowerSupport.MethodBaseToIl2CppMethodInfoPointer(
                typeof(ObjectPublicAbstractSealedDi2StTyHaDi2St1TyUnique)
                    .GetMethod("Method_Public_Static_Void_Component_0"));
           var myrealFunct = typeof(MonoBehaviour1).GetMethod("FakeMethod").MethodHandle.GetFunctionPointer();
           //MelonLogger.Log(realPtr);
            //MelonLogger.Log(myrealFunct);
            Imports.Hook(realPtr,myrealFunct);
        }

        public static void FakeMethod(Component aaaa)
        {
            // var TheTypes = GetTypes();
            // if(TheTypes == null) return;
            // if(!TheTypes.Any()) return;
            // if(aaaa == null) return;
            // foreach (var type in TheTypes)
            // {
            //     if (aaaa.GetType().IsAssignableFrom(type))
            //     {
            //         Object.Destroy(aaaa);
            //     }
            //}
        }
    }
}