using MelonLoader;
using PulsarCRepl;
using UIExpansionKit;
using UIExpansionKit.API;
using UnityEngine;
using VRJintMenuWrapper;

[assembly: MelonModInfo(typeof(MyMenuWrapper), "JintUnityUI", "1.0", "noah")]
[assembly: MelonModGame(null, null)]
namespace VRJintMenuWrapper
{
    public class MyMenuWrapper : MelonMod
    {
        private GameObject myUI;

        public override void OnApplicationStart()
        {
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.QuickMenu, "OpenJint", SpawnMenu);
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.QuickMenu, "CloseJint", CloseMenu);
        }

        public void CloseMenu()
        {
            myUI.SetActiveRecursively(false);
        }
        public void SpawnMenu()
        {
            if (myUI == null)
            {
                myUI = UnityAssetBundleUI.loadAsset();
                UnityAssetBundleUI.SetupMenu(myUI);
                myUI.transform.position = QuickMenu.prop_QuickMenu_0.gameObject.transform.position;
                myUI.transform.rotation = QuickMenu.prop_QuickMenu_0.gameObject.transform.rotation;
                myUI.transform.parent = QuickMenu.prop_QuickMenu_0.gameObject.transform;
            }
            else
            {
                myUI.SetActiveRecursively(true);
            }
        }
    }
}