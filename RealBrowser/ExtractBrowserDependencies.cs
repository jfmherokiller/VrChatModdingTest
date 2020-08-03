using System.IO;
using System.IO.Compression;
using MelonLoader.ICSharpCode.SharpZipLib.Core;
using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using ZipFile = MelonLoader.ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace RealBrowser
{
    public class ExtractBrowserDependencies
    {
        public static void ExtractContent()
        {
            var GameDataPath = Application.dataPath;
            var myzip = new ZipFile(new MemoryStream(Properties.myres.data));
            foreach (ZipEntry entry in myzip)
            {
                if (!entry.IsFile)
                {
                    continue;
                }

                var filename = entry.Name;
                var mybuff = new byte[4096];
                var mystream = myzip.GetInputStream(entry);
                var fullpath = Path.Combine(GameDataPath, filename);
                var mdirectoryname = Path.GetDirectoryName(fullpath);
                if (mdirectoryname.Length > 0)
                {
                    Directory.CreateDirectory(mdirectoryname);
                }

                using (var streamWriter = File.Create(fullpath))
                {
                    StreamUtils.Copy(mystream, streamWriter, mybuff);
                }
            }
        }
    }
}