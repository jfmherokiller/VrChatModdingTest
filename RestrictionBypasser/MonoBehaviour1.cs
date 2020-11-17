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

[assembly: MelonInfo(typeof(MonoBehaviour1), "RestrictionKiller", "1.0", "Author Name")]
[assembly: MelonGame(null, null)]
namespace RestrictionBypasser
{

    public class MonoBehaviour1 : MelonMod
    {
        public override void OnApplicationStart()
        {
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
           MelonLogger.Log(realPtr);
            MelonLogger.Log(myrealFunct);
            Imports.Hook(realPtr,myrealFunct);
        }

        public static void FakeMethod(Component aaaa)
        {
            return;
        }
    }
}