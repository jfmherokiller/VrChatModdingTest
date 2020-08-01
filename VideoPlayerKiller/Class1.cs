
using Il2CppSystem;
using MelonLoader;
using UnhollowerRuntimeLib;
using VideoPlayerKiller;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

[assembly: MelonModInfo(typeof(Class1), "VideoKiller", "1.0", "Author Name")]
[assembly: MelonModGame(null, null)]
namespace VideoPlayerKiller
{
    public class Class1 : MelonMod
    {
        public override void OnLevelWasLoaded(int level)
        {
            KillVideos();
        }

        public override void OnUpdate()
        {
            KillVideos();
        }

        public void KillVideos()
        {
            foreach (var vrcUiVideo in Resources.FindObjectsOfTypeAll(Il2CppType.Of<VRCUiVideo>()))
            {
                Object.Destroy(vrcUiVideo);
            }
        }
    }
}