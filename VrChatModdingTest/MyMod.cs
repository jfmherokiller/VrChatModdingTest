using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppSystem.Text;
using MelonLoader;
using SR_PluginLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC.SDKBase;
using ArgumentException = System.ArgumentException;
using Console = System.Console;

namespace VrChatModdingTest
{
    public class MyMod : MelonMod
    {
        public string MyModels;
        public AssetBundle[] AssetBundles = new AssetBundle[0];

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                //TestObj();
                TestBundles();
            }
        }

        private void TestObj()
        {
            MyModels = Path.Combine(Application.dataPath, "..", "Models");
            var ModelFiles = Il2CppSystem.IO.Directory.GetFiles(MyModels, "*.obj");
            foreach (var modelFile in ModelFiles)
            {
                FileStream fs = File.OpenRead(modelFile);
                var sack_prefab = ModelHelper.Create_Model_Prefab("sack_o_seeds", fs, resolve_model_mats,
                    new Model_Prefab_Transform[] {Model_Prefab_Transform.Flip_Z});
                Transform trans = sack_prefab.transform.Find("PHYS_0_Trigger");
                GameObject trigger = trans.gameObject;
                Collider col = trigger.GetComponent<Collider>();
                col.isTrigger = true;
                col.enabled = true;
                fs.Close();
            }
        }

        public void TestBundles()
        {
            MyModels = Path.Combine(Application.dataPath, "..", "Models");
            var ModelFiles = Il2CppSystem.IO.Directory.GetFiles(MyModels, "*.pro");
            foreach (var modelFile in ModelFiles)
            {
                var ReadFile = AssetBundle.LoadFromFile(modelFile);
                //AssetBundles = AddToArray(AssetBundles, ReadFile);
                
                try
                {
                    var Contents = ReadFile.LoadAllAssets(Il2CppType.Of<GameObject>());
                    foreach (var content in Contents)
                    {
                        var Output = UnityEngine.Object.Instantiate(content).TryCast<GameObject>();
                        var playerPos = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
                        Output.transform.position = playerPos;
                        Output.AddComponent<Rigidbody>();
                        
                        Output.AddComponent<VRC_Pickup>();
                        
                        ReadFile.Unload(false);
                        
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                   // var Bundles = AssetBundles;
                   // foreach (var bundle in Bundles)
                   // {
                   //     var contents = bundle.LoadAllAssets(Il2CppType.Of<GameObject>());
                   //     foreach (var content in contents)
                   //     {
                   //         UnityEngine.Object.Instantiate(content);
                   //     }
                   // }
                }
            }
        }

        public static Stream resolve_model_mats(string fileName)
        {
            var MyModels = Path.Combine(Application.dataPath, "..", "Models");
            string file = Path.GetFileName(fileName);
            var finalPath = Path.Combine(MyModels, file);
            FileStream fs = File.OpenRead(finalPath);
            return fs;
        }

        public override void OnApplicationStart()
        {
            //ClassInjector.RegisterTypeInIl2Cpp<ModelData>();
            //ClassInjector.RegisterTypeInIl2Cpp<ModelData_Header>();
        }
    }
}