using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Harmony;
using MelonLoader;
using RestrictionBypasser;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;
using Marshal = Il2CppSystem.Runtime.InteropServices.Marshal;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(MonoBehaviour1), "RestrictionKiller", "1.0", "Author Name")]
[assembly: MelonGame(null, null)]
namespace RestrictionBypasser
{

    public class MonoBehaviour1 : MelonMod
    {
        public static string settingsCategory = "CustomComponentBlockList";
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

        public static unsafe void FakeMethod(IntPtr aaaa)
        {
            var objectPtr = UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<Component>(aaaa);
            //var testIng = Il2CppType.TypeFromPointer(aaaa, nameof(UnityEngine.Component));
            if(objectPtr == null) return;
           // MelonLogger.Log(objectPtr.name);
            var TheTypes = GetTypes();
            if(TheTypes == null) return;
            if(!TheTypes.Any()) return;
            //var myobj = Marshal.PtrToStructure<Component>(aaaa);
            //MelonLogger.Log(&aaaa->name);
             foreach (var type in TheTypes)
             {
                 if (!objectPtr.GetType().IsAssignableFrom(type)) continue;
                 //MelonLogger.Log(objectPtr.name);
                 Object.Destroy(objectPtr);
             }
            //Object.Destroy(aaaa);
            
             //MelonLogger.Log(test);
            return;
        }
    }
}