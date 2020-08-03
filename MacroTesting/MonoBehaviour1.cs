using MacroTesting;
using UnityEngine;
using MelonLoader;
[assembly: MelonModInfo(typeof(MonoBehaviour1), "MacroTest", "1.0", "Author Name")]
[assembly: MelonModGame(null, null)]
namespace MacroTesting
{
    public class MonoBehaviour1 : MelonMod
    {

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.localScale += new Vector3(1,1,1);
                VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Internal_Animator_0.Rebind();
                VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.field_Internal_IkController_0.HeadEffector.transform.position += new Vector3(1,1,1);
            }
        }
    }
}