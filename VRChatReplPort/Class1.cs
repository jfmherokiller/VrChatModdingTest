
using System;
using System.Collections;
using System.Threading.Tasks;
using MelonLoader;

using PulsarCRepl;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: MelonModInfo(typeof(PulsarCRepMod), "JintDebugConsoleBase", "1.0", "Noah")]
[assembly: MelonModGame(null, null)]
namespace PulsarCRepl
{
    
    public class PulsarCRepMod : MelonMod
    {
        public static GameObject myUI;
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

        public static JintInstance MakeNewJintInstance()
        {
            var myinstance = new JintInstance();
            myinstance.SetupEnginePieces();
            return myinstance;
        }

        public override void OnUpdate()
        {
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