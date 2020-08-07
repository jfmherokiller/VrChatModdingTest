using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRCSDK2;
using WebrowserTesting;

[assembly: MelonInfo(typeof(Classone), "WebBrowserSpawnTest", "1.0", "Author Name")]
[assembly: MelonGame(null, null)]
namespace WebrowserTesting
{
    public class Classone : MelonMod
    {
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                var mygame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mygame.AddComponent<Canvas>();
                //mygame.transform.rotation = Quaternion.AngleAxis(90,Vector3.forward);
                //mygame.AddComponent<Rigidbody>();
                //mygame.AddComponent<VRC_Pickup>();
                var myCanvas = mygame.GetComponent<Canvas>().TryCast<Canvas>();
                myCanvas.renderMode = RenderMode.WorldSpace;
                mygame.AddComponent<CanvasScaler>();
                mygame.AddComponent<GraphicRaycaster>();
                var myUIShap = mygame.AddComponent<VRC_UiShape>();
                var mypanel = mygame.AddComponent<VRC_WebPanel>();
                mygame.transform.position = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
                mypanel.interactive = true;
                mypanel.defaultUrl = "https://www.mirc.com/servers.ini";
                mypanel.enabled = true;    
            }
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
        }
    }
}