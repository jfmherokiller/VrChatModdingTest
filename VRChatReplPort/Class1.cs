
using System;
using System.Threading.Tasks;
using MelonLoader;

using PulsarCRepl;
using UnhollowerRuntimeLib;
using UnityEngine;

[assembly: MelonModInfo(typeof(PulsarCRepMod), "PulsarCheats2", "1.0", "Author Name")]
[assembly: MelonModGame(null, null)]
namespace PulsarCRepl
{
    
    public class PulsarCRepMod : MelonMod
    {
        public void mycode()
        {
            JintBits.RunMe();
        }
        public override void OnApplicationStart()
        {
            //StartConsoleBasedRepl();
            //MelonModLogger.Log("Loaded JS");
            ClassInjector.RegisterTypeInIl2Cpp<JintConsoleGui>();
        }

        private void StartConsoleBasedRepl()
        {
            Task.Run(mycode);
        }

        public override void OnLevelWasLoaded(int level)
        {
            var myobject = new GameObject();
            myobject.AddComponent<JintConsoleGui>();
        }
    }

    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            CosturaUtility.Initialize();
        }
    }
}