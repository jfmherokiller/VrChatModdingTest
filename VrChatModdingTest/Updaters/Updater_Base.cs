using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SR_PluginLoader
{
    public enum UPDATER_TYPE
    {
        NONE = 0,
        JSON,// 
        GIT,// Git system
        WEB// Simple web download
    }

    public enum FILE_UPDATE_STATUS
    {
        /// <summary>
        /// This file is perfectly up to date
        /// </summary>
        UP_TO_DATE=0,
        /// <summary>
        /// The file has an updated version available.
        /// </summary>
        OUT_OF_DATE,
        /// <summary>
        /// This file seems to be a developer compiled one, there is no history record for it. (mostly returned from github updaters)
        /// </summary>
        DEV_FILE,
        /// <summary>
        /// There was an error getting the file update status.
        /// </summary>
        ERROR,
        /// <summary>
        /// The remote file could not be located.
        /// </summary>
        NOT_FOUND
    }

    public delegate bool Updater_File_Type_Confirm(string ContentType);
    public delegate void Updater_File_Download_Progress(float read, float total_bytes);
    public delegate void Updater_File_Download_Completed(string filename);
    public delegate void Updater_Cache_Or_Open_File_Callback(FileStream stream);
    public delegate void Updater_Get_Result(byte[] buf);

    public abstract class Updater_Base
    {
        protected static Encoding ENCODING = Encoding.UTF8;
        protected const int CHUNK_SIZE = 2048;

        public static HttpWebRequest CreateRequest(string url)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            System.Security.Cryptography.AesCryptoServiceProvider b = new System.Security.Cryptography.AesCryptoServiceProvider();

            Uri uri = new Uri(url);
            HttpWebRequest webRequest = WebRequest.Create(uri) as HttpWebRequest;
            webRequest.UserAgent = USER_AGENT;
            return webRequest;
        }

        public static Updater_Base Get_Instance(UPDATER_TYPE ty)
        {
            switch (ty)
            {
                case UPDATER_TYPE.GIT:
                    return Git_Updater.instance;
                case UPDATER_TYPE.WEB:
                    return Web_Updater.instance;
            }

            return null;
        }
        public static readonly string USER_AGENT = "SR_Plugin_Loader  on GitHub";
        protected static Dictionary<string, byte[]> remote_file_cache = new Dictionary<string, byte[]>();

        // This function is used by plugins, they pass their given update info url and the path to their current .dll file
        // How these are interpreted varies for each updater type.
        // The GIT updater actually searches the repository master branch and compares the current hash for the given file on the repo to the one on the users system
        // The JSON updater (when someone gets around to making it) will take a URL that leads to a JSON file containing the information for a single plugin, that json information should contain at the very LEAST: two fields (HASH, DOWNLOAD)
        //      HASH: is the git-format-sha1 hash for the currently released version of the plugin
        //      DOWNLOAD: is a direct URL link to the currently released version of the plugin DLL
        public virtual FILE_UPDATE_STATUS Get_Update_Status(string remote_file, string local_file)
        {
            return FILE_UPDATE_STATUS.DEV_FILE;
        }


        public static byte[] Get(string url)
        {
            HttpWebRequest webRequest = CreateRequest(url);
            WebResponse webResponse = webRequest.GetResponse();
            Stream stream = webResponse.GetResponseStream();

            int total = (int)webResponse.ContentLength;
            byte[] buf = new byte[total];

            int read = 0;//how many bytes we have read so far (offset within the stream)
            int remain = total;//how many bytes are left to read
            int r = 0;

            while (remain > 0)
            {
                r = stream.Read(buf, read, Math.Min(remain, CHUNK_SIZE));
                read += r;
                remain -= r;
            }

            return buf;
        }

        public static string GetString(string url)
        {
            byte[] buf = Get(url);
            if (buf == null) return null;
            if (buf.Length <= 0) return "";

            return ENCODING.GetString(buf);
        }

        // Remember: when this function is used by plugins they will pass their given updater_method URL for 'remote_file'
        // In the case of things like the Git updater this is fine as that url will BE a link to the most current version of the plugin DLL
        // However in the case of the JSON updater that url will instead be a link to the JSON file containing the HASH and DOWNLOAD URL for said plugin.
        // So for the JSON updater this method needs to be overriden and made to download that JSON info and treat the DOWNLOAD url contained therein as if IT was passed as the 'remote_file'
        public virtual IEnumerator DownloadAsync(string remote_file, string local_file, Updater_File_Type_Confirm confirm = null, Updater_File_Download_Progress prog_callback = null, Updater_File_Download_Completed download_completed = null)
        {
            SLog.Debug("Downloading: {0}", remote_file);
            if (local_file == null) local_file = String.Format("{0}\\{1}", UnityEngine.Application.dataPath, Path.GetFileName(remote_file));

            WebResponse resp = null;
            Stream stream = null;

            HttpWebRequest webRequest = CreateRequest(remote_file);

            WebAsync webAsync = new WebAsync();
            IEnumerator e = webAsync.GetResponse(webRequest);
            while (e.MoveNext()) { yield return e.Current; }// wait for response to arrive
            while (!webAsync.isResponseCompleted) yield return null;// double check for clarity & safety

            RequestState result = webAsync.requestState;
            resp = result.webResponse;

            if (confirm != null)
            {
                if (confirm(resp.ContentType) == false)
                {
                    yield break;//exit routine
                }
            }

            stream = resp.GetResponseStream();
            int total = (int)resp.ContentLength;
            byte[] buf = new byte[total];

            int read = 0;//how many bytes we have read so far (offset within the stream)
            int remain = total;//how many bytes are left to read
            int r = 0;

            while (remain > 0)
            {
                r = stream.Read(buf, read, Math.Min(remain, CHUNK_SIZE));
                read += r;
                remain -= r;
                if (prog_callback != null)
                {
                    try
                    {
                        prog_callback(read, total);
                    }
                    catch (Exception ex)
                    {
                        SLog.Error(ex);
                    }
                }
                yield return 0;// yield execution until next frame
            }

            // It's good practice when overwriting files to write the new version to a temporary location and then copy it overtop of the original.
            string temp_file = String.Format("{0}.temp", local_file);
            File.WriteAllBytes(temp_file, buf);
            File.Copy(temp_file, local_file, true);
            File.Delete(temp_file);// delete the now unneeded .temp file

            download_completed?.Invoke(local_file);
            yield break;//exit routine
        }

        /// <summary>
        /// Performs a coroutine Http GET request and returns a byte array with the result via both a callback and the current value of the IEnumerator object.
        /// </summary>
        public static IEnumerator GetAsync(string url, Updater_File_Type_Confirm confirm = null, Updater_File_Download_Progress prog_callback = null, Updater_Get_Result callback = null)
        {
            if (remote_file_cache.ContainsKey(url))
            {
                yield return remote_file_cache[url];
                yield break;
            }

            WebResponse resp = null;
            Stream stream = null;

            HttpWebRequest webRequest = CreateRequest(url);
            
            WebAsync webAsync = new WebAsync();
            IEnumerator e = webAsync.GetResponse(webRequest);
            if (e == null)
            {
                SLog.Info("Updater_Base.Get() Enumerator is NULL!");
                yield return null;
                yield break;
            }

            while (e.MoveNext()) { yield return null; }// wait for response to arrive
            while (!webAsync.isResponseCompleted) yield return null;// double check for clarity & safety

            RequestState result = webAsync.requestState;
            resp = result.webResponse;

            if (confirm != null)
            {
                if (confirm(resp.ContentType) == false)
                {
                    yield break;//exit routine
                }
            }

            stream = resp.GetResponseStream();
            int total = (int)resp.ContentLength;
            byte[] buf = new byte[total];

            int read = 0;//how many bytes we have read so far (offset within the stream)
            int remain = total;//how many bytes are left to read
            int r = 0;

            while (remain > 0)
            {
                r = stream.Read(buf, read, Math.Min(remain, CHUNK_SIZE));
                read += r;
                remain -= r;
                if (prog_callback != null)
                {
                    try
                    {
                        prog_callback(read, total);
                    }
                    catch (Exception ex)
                    {
                        SLog.Error(ex);
                    }
                }
                yield return null;// yield execution until next frame
            }

            if (!remote_file_cache.ContainsKey(url)) remote_file_cache.Add(url, buf);
            else remote_file_cache[url] = buf;

            callback?.Invoke(buf);
            yield return buf;
            yield break;
        }
    }
}
