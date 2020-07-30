using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VrChatModdingTest;

namespace SR_PluginLoader
{
    public static class ResourceExt
    {
        static Dictionary<string, Texture> loadedTextures = new Dictionary<string, Texture>();

        public static void map_SR_Icons()
        {
            Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();
            foreach (Texture tex in allTextures)
            {
                if (tex.name.StartsWith("icon"))
                {
                    loadedTextures.Add(tex.name, tex); //Store the found texture
                }
            }
        }

        /// <summary>
        /// Find texture by name, if the path is unknown. Warning: It is a slow process and uses a lot of memory.
        /// </summary>
        public static Texture FindTexture(string name)
        {
            Texture result;
            if (loadedTextures.TryGetValue(name, out result))
            {
                return result; //Already loaded the texture
            }
            else
            {
                //Search in all the textures that been loaded by unity
                Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();
                foreach (Texture tex in allTextures)
                {
                    if (tex.name == name)
                    {
                        loadedTextures.Add(name, tex); //Store the found texture
                        return tex;
                    }
                }
                
                loadedTextures.Add(name, null);
                SLog.Error("Could not find texture: " + name);
                return null;
            }
        }

        /// <summary>
        /// Outputs a list of all loaded Unity Shaders.
        /// </summary>
        public static void Print_All_Shaders()
        {
            var all = Resources.FindObjectsOfTypeAll<Shader>();
            List<string> list = new List<string>();

            SLog.Info("=== SHADERS ===");
            foreach(Shader shader in all)
            {
                list.Add(shader.name);
                SLog.Info("  - {0}", shader.name);
            }
            SLog.Info("===============");

            System.IO.File.WriteAllLines("shader_list.txt", list.ToArray());
        }
    }
}
