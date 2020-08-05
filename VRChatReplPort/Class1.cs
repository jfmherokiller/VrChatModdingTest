
using System;
using System.Collections;
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
        public static JintInstance myinstance;
        public void mycode()
        {
            JintBits.RunMe();
        }
        public override void OnApplicationStart()
        {
            //StartConsoleBasedRepl();
            //MelonModLogger.Log("Loaded JS");
            myinstance = new JintInstance();
            myinstance.SetupEnginePieces();
            ClassInjector.RegisterTypeInIl2Cpp<JintConsoleGui>();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                bool flag = typeof(ICollection).IsAssignableFrom(typeof(Vector3));
            }
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