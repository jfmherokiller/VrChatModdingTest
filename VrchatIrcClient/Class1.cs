using MelonLoader;
using UIExpansionKit.API;
using UnhollowerRuntimeLib;
using UnityEngine;
using VrchatIrcClient;

[assembly: MelonModInfo(typeof(Class1), "IrcClient", "1.0", "Author Name")]
[assembly: MelonModGame(null, null)]
namespace VrchatIrcClient
{

    public class Class1 : MelonMod
    {
        public static IrcInstance myistance;
        public static IrcConsoleGui mygui;
        public override void OnApplicationStart()
        {
            myistance = new IrcInstance();
            ClassInjector.RegisterTypeInIl2Cpp<IrcConsoleGui>();
        }
        public override void OnLevelWasLoaded(int level)
        {
            var myobject = new GameObject().TryCast<GameObject>();
            mygui = myobject.AddComponent<IrcConsoleGui>();
        }

        public override void OnUpdate()
        {
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