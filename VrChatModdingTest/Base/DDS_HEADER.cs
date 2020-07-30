using System;
using System.Runtime.InteropServices;

namespace SR_PluginLoader
{
    //http://doc.51windows.net/Directx9_SDK/?url=/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm

    using DWORD = UInt32;
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct DDS_HEADER
    {
        public DWORD MAGIC;

        public DWORD DSIZE;
        public DWORD FLAGS;
        public DWORD dwHeight;
        public DWORD dwWidth;
        public DWORD dwPitchOrLinearSize;
        public DWORD dwDepth;
        public DWORD dwMipMapCount;
        public DWORD dwReserved_1;
        public DWORD dwReserved_2;
        public DWORD dwReserved_3;
        public DWORD dwReserved_4;
        public DWORD dwReserved_5;
        public DWORD dwReserved_6;
        public DWORD dwReserved_7;
        public DWORD dwReserved_8;
        public DWORD dwReserved_9;
        public DWORD dwReserved_10;
        public DWORD dwReserved_11;
        public DDPIXELFORMAT pixelFormat;
        public DDSCAPS ddsCaps;
        public DWORD dwResrv;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct DDPIXELFORMAT
    {
        /// <summary>
        /// Size of this structure, MUST be 32!
        /// </summary>
        public DWORD dwSize;
        public DWORD dwFlags;
        public DWORD dwFourCC;
        public DWORD dwRGBBitCount;
        public DWORD dwRBitMask;
        public DWORD dwGBitMask;
        public DWORD dwBBitMask;
        public DWORD dwRGBAlphaBitMask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct DDSCAPS
    {
        public DWORD ddsCaps1;
        public DWORD ddsCaps2;
        public DWORD whocares_1;
        public DWORD whocares_2;
    }
    
    public static class DXT
    {
        public static DWORD MAKEFOURCC(char ch0, char ch1, char ch2, char ch3)
        {
            return ((DWORD)(byte)(ch0) | ((DWORD)(byte)(ch1) << 8) | ((DWORD)(byte)(ch2) << 16) | ((DWORD)(byte)(ch3) << 24));
        }
    }
}
