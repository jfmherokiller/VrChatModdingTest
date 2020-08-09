using UnhollowerRuntimeLib;
using UnityEngine;

namespace VRChatReplPort
{
    public static class AssetBundleUtils {
        public static AssetBundle LoadAssetBundle(string filePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile_Internal(filePath, 0, 0);
            assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return assetBundle;
        }

        public static T LoadAsset<T>(AssetBundle assetBundle, string assetPath) where T : Object {
            var asset = assetBundle.LoadAsset_Internal(assetPath, Il2CppType.Of<T>()).Cast<T>();
            asset.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return asset;
        }

        public static GameObject Instantiate(GameObject gameObject)
        {
            GameObject instantiatedGameObject = Object.Instantiate(gameObject);
            instantiatedGameObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return instantiatedGameObject;
        }
    }
}