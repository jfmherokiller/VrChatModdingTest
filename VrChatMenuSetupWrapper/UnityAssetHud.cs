using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.SDKBase;
using VRChatReplPort;
using Object = UnityEngine.Object;

namespace PulsarCRepl
{
    // Token: 0x02000174 RID: 372
    public class UnityAssetBundleUI
    {
        // Token: 0x040004B0 RID: 1200
        public static Action mystuff;

        // Token: 0x040004B1 RID: 1201
        public static Text codeOutputPanel;

        // Token: 0x040004B2 RID: 1202
        public static InputField codeInputPanel;
        public static JintInstance myinstance = PulsarCRepMod.MakeNewJintInstance();

        // Token: 0x06000A17 RID: 2583 RVA: 0x000021E3 File Offset: 0x000003E3
        public UnityAssetBundleUI()
        {
        }

        // Token: 0x06000A12 RID: 2578 RVA: 0x0003DD0C File Offset: 0x0003BF0C
        public static GameObject loadAsset()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var name = executingAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith("mymenu"));
            var manifestResourceStream = executingAssembly.GetManifestResourceStream(name);
            var arr = StreamToByteArray(manifestResourceStream);
            var assetBundle = AssetBundle.LoadFromMemory(arr);
            return AssetBundleUtils.Instantiate(AssetBundleUtils.LoadAsset<GameObject>(assetBundle, "Assets/Scene/Scenes/MyMenu.prefab"));
        }

        // Token: 0x06000A13 RID: 2579 RVA: 0x0003DD80 File Offset: 0x0003BF80
        public static byte[] StreamToByteArray(Stream inputStream)
        {
            var array = new byte[inputStream.Length];
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = inputStream.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, count);
                }

                result = memoryStream.ToArray();
            }

            return result;
        }

        // Token: 0x06000A14 RID: 2580 RVA: 0x0003DDEC File Offset: 0x0003BFEC
        public static void SetupMenu(GameObject myasset)
        {
            var button = myasset.GetComponentsInChildren<Button>().First();
            codeInputPanel = myasset.GetComponentsInChildren<InputField>().First();
            codeOutputPanel = GameObject.Find("CodeOutput").GetComponentInChildren<Text>().TryCast<Text>();
            var canvas = myasset.GetComponentsInChildren<Canvas>().First();
            var myUIShape = canvas.gameObject.AddComponent<VRC_UiShape>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener((UnityAction)sendCode);
        }

        // Token: 0x06000A15 RID: 2581 RVA: 0x0003DE78 File Offset: 0x0003C078
        public static void sendCode()
        {
            MelonModLogger.Log("testButton");
            myinstance.ExecuteCode(codeInputPanel.text);
            codeOutputPanel.text = myinstance.GetOutput();
        }
    }
}