using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using VrChatModdingTest;

namespace SR_PluginLoader
{
    public enum GRADIENT_DIR
    {
        LEFT_RIGHT,
        TOP_BOTTOM
    }


    public static class Util
    {
        public static int UnixTimestamp() { return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; }


        


        /// <summary>
        /// Gets a string listing information for all Components attached to all GameObjects attached to a specified GameObject.
        /// </summary>
        public static string Get_Unity_GameObject_Hierarchy_String(GameObject targ, int nest_level=0)
        {
            var sb = new StringBuilder();
            // Format our pre-spacing string
            var space = "";
            for (var i = 0; i < nest_level; i++) space += "   ";

            // First let's print this objects name
            sb.AppendFormat("{0}- \"{1}\"  Children<{2}>", space, targ.name, targ.transform.childCount);

            var comps = targ.GetComponents<Component>().ToList();
            sb.Append("  Components("+comps.Count+"): {");
            // Now let's list all of the script components attached to it
            sb.Append(String.Join(", ", comps.Select(c => c.GetType().Name).ToArray()));
            // End the components list
            sb.AppendLine("}");

            if (nest_level >= 9) return sb.ToString().TrimEnd(new char[] {'\n'});
            // Start the process of listing all attached GameObjects
            for (var idx = 0; idx < targ.transform.childCount; idx++)
            {
                var trans = targ.transform.GetChild(idx);
                if (trans.gameObject == trans.parent || trans.parent == null) continue;
                var str = Get_Unity_GameObject_Hierarchy_String(trans.gameObject, nest_level + 1);
                if(!String.IsNullOrEmpty(str)) sb.AppendLine(str.TrimEnd(new char[] { '\n' }));
            }

            return sb.ToString().TrimEnd(new char[] { '\n' });
        }


        #region Hashing

        public static string SHA(string data)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(data));

            var sb = new StringBuilder();
            foreach (var hashByte in hash)
            {
                sb.Append(hashByte.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string SHA(string format, params object[] args)
        {
            var data = String.Format(format, args);
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(data));

            var sb = new StringBuilder();
            foreach (var HashByte in hash)
            {
                sb.Append(HashByte.ToString("x2"));
            }

            return sb.ToString();
        }
        
        public static string Git_File_Sha1_Hash(string file)
        {
            const string hash_empty = "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391"; //this is the correct hash that should be given for an empty file.
            if (!File.Exists(file)) return hash_empty;

            var buf = File.ReadAllBytes(file);
            return Git_Blob_Sha1_Hash(buf);
        }

        public static string Git_Blob_Sha1_Hash(byte[] buf)
        {
            // Encoding.ASCII is 7bit encoding but we want 8bit, so we need to use "iso-8859-1"
            var enc = Encoding.GetEncoding("iso-8859-1");
            var head = String.Format("blob {0}\0{1}", buf.Length, enc.GetString(buf));

            /*
            StringBuilder dat = new StringBuilder();
            dat.Append(head);
            dat.Append(Encoding.ASCII.GetString(buf));
            //PLog.Info("HEAD: size({0}) content: '{1}'", dat.Length, dat.ToString());
            byte[] blob_buf = Encoding.ASCII.GetBytes(dat.ToString());
            */

            var sha = SHA1.Create();
            var hash = sha.ComputeHash( enc.GetBytes(head) );

            var sb = new StringBuilder();
            foreach (var HashByte in hash)
            {
                sb.Append(HashByte.ToString("x2"));
            }

            const string hash_foobar = "323fae03f4606ea9991df8befbb2fca795e648fa"; // Correct GIT hash for a file containing only "foobar\n"
            var match = (String.CompareOrdinal(sb.ToString(), hash_foobar)==0);
            //PLog.Info("[SHA1 HASH TEST] Match<{0}>  Hash: {1}  HEAD: '{2}'", (match?"TRUE":"FALSE"), sb.ToString(), head);

            return sb.ToString();
        }
        #endregion

        public static void Log_Resource_Names()
        {
            var thisExe = System.Reflection.Assembly.GetCallingAssembly();
            var resources = thisExe.GetManifestResourceNames();

            foreach(var res in resources)
            {
                SLog.Info(res);
            }
        }

        public static Stream Get_Resource_Stream(string name, string namespace_str)
        {
            var asset_name = $"{namespace_str}.Resources.{name}";
            return Assembly.GetCallingAssembly().GetManifestResourceStream(asset_name);
        }

        public static byte[] Load_Resource(string name, string namespace_str = "SR_PluginLoader")
        {
            try
            {
                using (var stream = Get_Resource_Stream(name, namespace_str))
                {
                    if (stream == null) return null;

                    var buf = new byte[stream.Length];
                    var read = stream.Read(buf, 0, (int)stream.Length);
                    if (read >= (int) stream.Length) return buf;
                    var remain = ((int)stream.Length - read);
                    var r = 0;
                    while (r < remain && remain > 0)
                    {
                        r = stream.Read(buf, read, remain);
                        read += r;
                        remain -= r;
                    }

                    return buf;
                }
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
                return null;
            }
        }

        public static byte[] Read_Stream(Stream stream)
        {
            if (stream == null) return null;

            var buf = new byte[stream.Length];
            var read = stream.Read(buf, 0, (int)stream.Length);
            if (read >= (int) stream.Length) return buf;
            var remain = ((int)stream.Length - read);
            var r = 0;
            while (r < remain && remain > 0)
            {
                r = stream.Read(buf, read, remain);
                read += r;
                remain -= r;
            }

            return buf;
        }

        #region TEXTURES
        [Obsolete("Use TextureHelper.Load_From_Resource instead!", true)]
        public static Texture2D Load_Texture_Resource(string name, string namespace_str)
        {
            return (Texture2D)TextureHelper.Load_From_Resource(name, namespace_str);
        }

        [Obsolete("Use TextureHelper.Load_From_Data instead!", true)]
        public static Texture2D Load_Texture_From_Data(byte[] data)
        {
            return (Texture2D)TextureHelper.Load(data);
        }
        /// <summary>
        /// Helper function to load an array of bytes as a struct instance. God I wish I had done this whole loader in C++
        /// </summary>
        public static T BytesToStructure<T>(byte[] bytes)
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        #endregion

        #region UI Helpers



        public static float Lerp(float a, float b, float f)
        {
            var fv = Math.Abs(f);
            return (a * fv) + (b * (1f - fv));
        }
        
        /// <summary>
        /// Create a gradient texture from with a base color other then white-black
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="dir"></param>
        /// <param name="shade1"></param>
        /// <param name="shade2"></param>
        /// <param name="clr"></param>
        /// <param name="exponential"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static Texture2D Get_Gradient_Texture(int pixels, GRADIENT_DIR dir, float shade1, float shade2, Color clr, bool exponential = false, float exponent = 1f)
        {
            var clr1 = new Color(shade1 * clr.r, shade1 * clr.g, shade1 * clr.b, clr.a);
            var clr2 = new Color(shade2 * clr.r, shade2 * clr.g, shade2 * clr.b, clr.a);
            return Get_Gradient_Texture(pixels, dir, clr1, clr2, exponential, exponent);
        }

        public static Texture2D Get_Gradient_Texture(int pixels, GRADIENT_DIR dir, float shade1, float shade2, bool exponential = false, float exponent = 1f)
        {
            var clr1 = new Color(shade1, shade1, shade1);
            var clr2 = new Color(shade2, shade2, shade2);
            return Get_Gradient_Texture(pixels, dir, clr1, clr2, exponential, exponent);
        }

        public static Texture2D Get_Gradient_Texture(int pixels, GRADIENT_DIR dir, Color clr1, Color clr2, bool exponential = false, float exponent = 1f)
        {
            var xsz = 1;
            var ysz = 1;
            if (dir == GRADIENT_DIR.TOP_BOTTOM) ysz = pixels;
            else xsz = pixels;

            var texture = new Texture2D(xsz, ysz, TextureFormat.RGBA32, true);

            for (var i = 0; i < pixels; i++)
            {
                var t = ((float)i / (float)pixels);
                if (exponential) t = (float)Math.Pow((double)t, exponent);
                if (dir == GRADIENT_DIR.TOP_BOTTOM) t = (1f - t);

                var clr = Color.Lerp(clr1, clr2, t);
                /*
                float R = Lerp(clr1.r, clr2.r, t);
                float G = Lerp(clr1.g, clr2.g, t);
                float B = Lerp(clr1.b, clr2.b, t);
                float A = Lerp(clr1.a, clr2.a, t);
                */

            var x = 0;
                var y = 0;

                if (dir == GRADIENT_DIR.TOP_BOTTOM) y = i;
                else x = i;

                texture.SetPixel(x, y, clr);
                //texture.SetPixel(x, y, new Color(R, G, B, A));
            }

            texture.anisoLevel = 4;
            texture.filterMode = FilterMode.Trilinear;
            texture.Apply(true,false);
            return texture;
        }


        public delegate Color PixelColoringDelegate(int x, int y, int width, int height);
        public static Texture2D Create_Texture(int width, int height, PixelColoringDelegate func)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, func(x, y, width, height));
                }
            }

            texture.anisoLevel = 4;
            texture.filterMode = FilterMode.Trilinear;
            texture.Apply(true,false);
            return texture;
        }

        public static Texture2D Create_Sheen_Texture(int size, Color tint)
        {
            return Util.Create_Texture(1, size, (int x, int y, int w, int h) => {
                var f = ((float)y / (float)h);
                var g = Util.Lerp(0.25f, 0.15f, f);
                if (y >= (h - 35)) g += 0.25f;

                return new Color(tint.r*g, tint.g*g, tint.b*g, tint.a);
            });
        }



        public static Texture2D Tint_Texture(Texture2D tex, Color clr)
        {
            for(var x=0; x<tex.width; x++)
            {
                for (var y = 0; y < tex.height; y++)
                {
                    var p = tex.GetPixel(x, y);
                    p *= clr;
                    tex.SetPixel(x, y, p);
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion

        public static bool floatEq(float a, float b, float epsilon = 0.001f)
        {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < float.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.Epsilon);
            }
            else { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public static string FormatByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }

            sb.Append("}");
            return sb.ToString();
        }

        public static string JSON_Escape_String(string str)
        {
            return str.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }



        #region Item Spawning
        

        #endregion
        

        #region Prefab Injection

        #endregion

        #region Mesh


        #endregion

    }
}
