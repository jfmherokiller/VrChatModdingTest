using MelonLoader;
using RealBrowser;
using UnhollowerRuntimeLib;
using UnityEngine;
[assembly: MelonModInfo(typeof(MonoBehaviour1), "WebrowserIl2cpp64bit", "1.0", "Author Name")]
[assembly: MelonModGame(null, null)]
namespace RealBrowser
{
    public class MonoBehaviour1 : MelonMod
    {
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<OffscreenCEF>();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                var mygame = GameObject.CreatePrimitive(PrimitiveType.Quad).TryCast<GameObject>();
                var myscript = mygame.AddComponent<OffscreenCEF>().TryCast<OffscreenCEF>();
                mygame.transform.position = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
            }
        }
    }
    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            ExtractBrowserDependencies.ExtractContent();
            CosturaUtility.Initialize();
        }
    }
}