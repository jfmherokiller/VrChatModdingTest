using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using VrChatModdingTest;

namespace SR_PluginLoader
{
    [Flags]
    public enum TextureOpFlags
    {
        None = (1 << 0),
        /// <summary>
        /// Doesnt disable mipmap generation, but rather sets the mipmap bias to a negative value such that nothing but the highest mipmap level is used (the texture originally uploaded)
        /// </summary>
        NO_MIPMAPPING = (1 << 1),
        /// <summary>
        /// Sets the texture wrap mode to CLAMP so it does not repeat.
        /// </summary>
        NO_WRAPPING = (1 << 2),
    }
    /// <summary>
    /// Assists with common tasks for loading textures into unity.
    /// </summary>
    public static class TextureHelper
    {
        private static int randIdx = 0;
        #region Common Textures
        private static Texture2D _transparent = null;
        public static Texture TRANSPARENT { get {
            if (_transparent != null) return _transparent;
            _transparent = new Texture2D(1, 1); _transparent.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f)); _transparent.Apply();
            return _transparent; } }

        public static Texture2D icon_unknown = null;
        public static Texture2D icon_alert = null;
        public static Texture2D icon_close = null;
        public static Texture2D icon_close_dark = null;
        public static Texture2D icon_logo = null;
        public static Texture2D icon_logo_sad = null;
        public static Texture2D icon_checkbox = null;
        public static Texture2D icon_checkmark = null;
        public static Texture2D icon_arrow_left = null;
        public static Texture2D icon_arrow_right = null;
        public static Texture2D icon_node_arrow_right = null;
        public static Texture2D icon_node_arrow_down = null;
        #endregion



        #region Public Functions

        /// <summary>
        /// Assists in loading a texture from a file that is an embedded resource.
        /// </summary>
        /// <param name="name">The embedded resource name.</param>
        /// <param name="namespace_str">The main namespace of the calling Assembly.</param>
        /// <returns></returns>
        public static Texture Load_From_Resource(string name, string namespace_str)
        {
            var asset_name = String.Format("{0}.Resources.{1}", namespace_str, name);
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(asset_name))
            {
                var data = Util.Read_Stream(stream);
                if (data == null) SLog.Info("UNABLE TO LOAD SPECIFIED RESOURCE: {0}", name);
                return Load(data, name);
            }
        }

        /// <summary>
        /// Assists in loading a texture from a file that is an embedded resource.
        /// </summary>
        /// <param name="name">The embedded resource name.</param>
        /// <param name="namespace_str">The main namespace of the calling Assembly.</param>
        /// <param name="flags">A set of flags from the <c>TextureOpFlags</c> enum.</param>
        /// <returns></returns>
        public static Texture Load_From_Resource(string name, string namespace_str, TextureOpFlags flags)
        {
            var asset_name = String.Format("{0}.Resources.{1}", namespace_str, name);
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(asset_name))
            {
                var data = Util.Read_Stream(stream);
                if (data == null) SLog.Info("UNABLE TO LOAD SPECIFIED RESOURCE: {0}", name);
                return Load(data, flags, name);
            }
        }

        /// <summary>
        /// Assists in loading a texture from a byte array.
        /// </summary>
        public static Texture Load(byte[] data, string name = null)
        {
            if (data == null || data.Length <= 0) return null;

            Texture2D tex = null;
            //Determine our file type
            var type = Identify_Texture_Type(data);
            switch (type)
            {
                case TextureType.DXT:
                    Load_Texture_DXT(out tex, data);
                    break;
                case TextureType.PNG:
                    Load_Texture_Non_DXT(out tex, data);
                    break;
                case TextureType.JPEG:
                    Load_Texture_Non_DXT(out tex, data);
                    break;
                default:
                    throw new NotImplementedException("Unable to determine that the given file was of a supported format!");
            }

            if (tex != null)// Suspected that at some point the 'tex' var would become a bad pointer to another texture and setting filterMode or ansioLevel was causing GUIStyle text character maps to render reallyyyy blurry, distorted, and misaligned. odd.
            {// Likely would indicate a deeper problem elsewhere in the code, we shall see.
                // Just some default's that are nice to have in most cases.
                tex.wrapMode = TextureWrapMode.Repeat;
                //tex.filterMode = FilterMode.Trilinear;
                //tex.anisoLevel = 1;
            }

            if (tex != null && name!=null) tex.name = name;
            return tex;
        }

        /// <summary>
        /// Assists in loading a texture from a stream.
        /// </summary>
        public static Texture Load(Stream stream, string name = null)
        {
            if (stream == null) return null;

            var data = Util.Read_Stream(stream);
            return Load(data, name);
        }
        
        /// <summary>
        /// Assists in loading a texture from a byte array.
        /// </summary>
        /// <param name="data">The texture file data.</param>
        /// <param name="name">A name to assign the texture instance.</param>
        /// <param name="flags">A set of flags from the <c>TextureOpFlags</c> enum.</param>
        /// <returns></returns>
        public static Texture Load(byte[] data, TextureOpFlags flags, string name = null)
        {
            if (data == null || data.Length <= 0) return null;
            var tex = (Texture2D)TextureHelper.Load(data, name);
            Enforce_Flags(ref tex, flags);
            return tex;
        }

        internal static void Enforce_Flags(ref Texture2D tex, TextureOpFlags flags)
        {
            if ((flags & TextureOpFlags.NO_MIPMAPPING) == TextureOpFlags.NO_MIPMAPPING) tex.mipMapBias = -(tex.mipmapCount);
            if ((flags & TextureOpFlags.NO_WRAPPING) == TextureOpFlags.NO_WRAPPING) tex.wrapMode = TextureWrapMode.Clamp;
        }

        /// <summary>
        /// Creates a copy of a texture pixel-by-pixel, it's slow but reliable for DEBUG purposes.
        /// </summary>
        public static Texture2D Clone(Texture2D tex)
        {
            var clone = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
            for(var x=0; x<tex.width; x++)
            {
                for(var y=0; y<tex.height; y++)
                {
                    clone.SetPixel(x, y, tex.GetPixel(x,y));
                }
            }

            return clone;
        }

        #endregion

        #region Debug
        /// <summary>
        /// A Debugging function, Cycles through all Textures loaded into unity.
        /// </summary>
        public static Texture Get_Next_From_All()
        {
            Texture2D[] all = Resources.FindObjectsOfTypeAll<Texture2D>();
            if ((randIdx + 1) >= all.Length) randIdx = 0;

            return all[randIdx++];
        }
        #endregion

        #region Internal Functions

        internal enum TextureType
        {
            UNKNOWN = 0,
            PNG,
            JPEG,
            DXT,
        }

        internal static readonly byte[] DDS_HEADER = new byte[] { 0x44, 0x44, 0x53, 0x20 };// The series of bytes that indicate a particular file contains DXT encoded image data.
        internal static readonly byte[] PNG_HEADER = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };// The series of bytes that would indicate a particular file contians PNG image data.
        internal static readonly byte[] JPEG_MAGIC = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };// The magic number that identifies the start of a JPEG header.
        internal static readonly byte[] JPEG_HEADER = new byte[] { 0x4A, 0x46, 0x49, 0x46, 0x00, };// The series of bytes for a JPEG header that would indicate it contains image data and not another type of file.

        internal static TextureType Identify_Texture_Type(byte[] data)
        {
            //Determine our file type
            if (Check_Header(data, DDS_HEADER))// Is it A DDS?
                return TextureType.DXT;
            else if (Check_Header(data, PNG_HEADER))// Is it PNG?
                return TextureType.PNG;
            else if (Check_Header(data, JPEG_MAGIC) && Check_Header(data, JPEG_HEADER, 6))// Is it JPEG?
                return TextureType.JPEG;

            return TextureType.UNKNOWN;
        }

        private static void Load_Texture_Non_DXT(out Texture2D tex, byte[] data)
        {
            tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, data, false);
        }

        private static void Load_Texture_DXT(out Texture2D tex, byte[] ddsBytes)
        {
            var ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

            var header = Util.BytesToStructure<DDS_HEADER>(ddsBytes);
            var fourCC = header.pixelFormat.dwFourCC;
            //PLog.Info("DDPF_FOURCC: {0}", fourCC);

            var textureFormat = TextureFormat.DXT1;
            if (fourCC == DXT.MAKEFOURCC('D', 'X', 'T', '1')) textureFormat = TextureFormat.DXT1;
            else if (fourCC == DXT.MAKEFOURCC('D', 'X', 'T', '5')) textureFormat = TextureFormat.DXT5;

            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");


            var height = ddsBytes[13] * 256 + ddsBytes[12];
            var width = ddsBytes[17] * 256 + ddsBytes[16];

            const int DDS_HEADER_SIZE = 128;
            var dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            tex = new Texture2D(width, height, textureFormat, false) {filterMode = FilterMode.Trilinear};
            tex.LoadRawTextureData(dxtBytes);
            tex.Apply();
            tex.Compress(true);
        }

        #endregion

        #region Internal Helpers
        private static bool Check_Header(byte[] data, byte[] magic, int offset = 0)
        {
            if (magic == null || data == null || magic.Length <= 0 || (data.Length - offset) <= 0 || magic.Length > (data.Length - offset))
            {
                SLog.Info("Bad arguments.");
                return false;
            }

            for (var i = 0; i < magic.Length; i++)
                if (data[i + offset] != magic[i])
                    return false;

            return true;
        }
        #endregion

        #region Extensions
        public static Texture getTexture(this Sprite sprite)
        {
            return ResourceExt.FindTexture(sprite.name);
        }
        #endregion

        internal static void Setup()
        {
            Load_Common();
        }

        /// <summary>
        /// Loads all of the predefined common texture resources.
        /// </summary>
        internal static void Load_Common()
        {
            const string myNamespace = "SR_PluginLoader";
            var FLAGS = (TextureOpFlags.NO_MIPMAPPING & TextureOpFlags.NO_WRAPPING);

            TextureHelper.icon_logo = (Texture2D)TextureHelper.Load_From_Resource("logo.png", myNamespace, FLAGS);
            TextureHelper.icon_logo_sad = (Texture2D)TextureHelper.Load_From_Resource("logo_sad.png", myNamespace, FLAGS);
            TextureHelper.icon_unknown = (Texture2D)TextureHelper.Load_From_Resource("mystery.png", myNamespace, FLAGS);
            TextureHelper.icon_alert = (Texture2D)TextureHelper.Load_From_Resource("alert.png", myNamespace, FLAGS);
            TextureHelper.icon_close = (Texture2D)TextureHelper.Load_From_Resource("close.png", myNamespace, FLAGS);
            TextureHelper.icon_close_dark = (Texture2D)TextureHelper.Load_From_Resource("close.png", myNamespace, FLAGS);
            TextureHelper.icon_checkbox = (Texture2D)TextureHelper.Load_From_Resource("checkbox.png", myNamespace, FLAGS);
            TextureHelper.icon_checkmark = (Texture2D)TextureHelper.Load_From_Resource("checkmark.png", myNamespace, FLAGS);

            TextureHelper.icon_arrow_left = (Texture2D)TextureHelper.Load_From_Resource("arrow_left.png", myNamespace, FLAGS);
            TextureHelper.icon_arrow_right = (Texture2D)TextureHelper.Load_From_Resource("arrow_right.png", myNamespace, FLAGS);

            TextureHelper.icon_node_arrow_right = (Texture2D)TextureHelper.Load_From_Resource("node_arrow.png", myNamespace, FLAGS);
            TextureHelper.icon_node_arrow_down = Rotate_90(icon_node_arrow_right, FLAGS);

            Util.Tint_Texture(TextureHelper.icon_close_dark, new Color(1f, 1f, 1f, 0.5f));
        }

        #region Texture Manipulation

        /// <summary>
        /// Creates a copy of a given texture rotated 90 degrees Clockwise.
        /// </summary>
        public static Texture2D Rotate_90(Texture2D tex, TextureOpFlags flags)
        {
            var cpy = new Texture2D(tex.width, tex.height);
            for (var x = 0; x < tex.width; x++)
            {
                for (var y = 0; y < tex.height; y++)
                {
                    var p = tex.GetPixel(x, y);
                    cpy.SetPixel(y, (tex.width - (x + 1)), p);
                }
            }

            cpy.Apply();
            Enforce_Flags(ref cpy, flags);
            return cpy;
        }
        
        public static Texture2D Flip_Horizontal(Texture2D tex, TextureOpFlags flags)
        {
            var cpy = new Texture2D(tex.width, tex.height);
            for (var x = 0; x < tex.width; x++)
            {
                for (var y = 0; y < tex.height; y++)
                {
                    var p = tex.GetPixel(x, y);
                    cpy.SetPixel((tex.width - (x + 1)), y, p);
                }
            }

            cpy.Apply();
            Enforce_Flags(ref cpy, flags);
            return cpy;
        }
        
        public static Texture2D Flip_Vertical(Texture2D tex, TextureOpFlags flags)
        {
            var cpy = new Texture2D(tex.width, tex.height);
            for (var x = 0; x < tex.width; x++)
            {
                for (var y = 0; y < tex.height; y++)
                {
                    var p = tex.GetPixel(x, y);
                    cpy.SetPixel(x, (tex.height-(y + 1)), p);
                }
            }

            cpy.Apply();
            Enforce_Flags(ref cpy, flags);
            return cpy;
        }
        #endregion
    }
}
