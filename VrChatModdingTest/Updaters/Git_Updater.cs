using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace SR_PluginLoader
{
    public class Git_Updater : Updater_Base
    {
        private static readonly Git_Updater _instance = new Git_Updater();
        public static Git_Updater instance { get { return _instance; } }
        public static readonly UPDATER_TYPE type = UPDATER_TYPE.GIT;
        private static SettingsFile Tracker = new SettingsFile("git_tracker");
        private static char[] PATH_SEP = new char[] { '\\', '/' };

        public delegate void Git_Updater_Repo_Result(JSONArray arr);

        //private static WebClient _webClient = null;
        //private static WebClient webClient { get { if (_webClient == null) { _webClient = GetClient(); } return _webClient; }  }

        private static string get_cname(string host)
        {
            var reg = new Regex(@"^(\w+\.)*github\.com$");
            Match match = reg.Match(host);
            
            return match.Groups[1].Value;
        }

        private static bool host_is_github(string host)
        {
            var reg = new Regex(@"^(\w+\.)*github\.com$");
            Match match = reg.Match(host);

            return match.Success;
        }

        private static string Extract_File_Path_From_Github_URL(string url)
        {
            var uri = new Uri(url);
            if (!host_is_github(uri.Host)) return uri.AbsolutePath;

            Regex reg = null;
            string cname = get_cname(uri.Host);
            if(String.Compare("raw.", cname)==0) reg = new Regex(@"^/\w+/\w+/\w+/(.+)$");// EX:  https://raw.github.com/dsisco11/SR_Plugin_Loader/master/Installer/SR_PluginLoader.dll
            else reg = new Regex(@"^/\w+/\w+/\w+/\w+/(.+)$");// EX:  https://github.com/dsisco11/SR_Plugin_Loader/raw/master/Plugins/SiloEnhancer.dll
            Match match = reg.Match(uri.AbsolutePath);
            //PLog.Info("match: success({0})  Group: {1}", (match.Success ? "TRUE" : "FALSE"), match.Groups[1].Value);

            if (match.Success) return match.Groups[1].Value;

            return url;
        }

        private static string Extract_Repository_URL_From_Github_URL(string url)
        {
            var uri = new Uri(url);
            if (!host_is_github(uri.Host)) return uri.AbsolutePath;
            string repo_url = null;

            if (String.Compare("repos", uri.Segments[1].TrimEnd(PATH_SEP)) == 0) { repo_url = String.Concat(uri.Segments[2].TrimEnd(PATH_SEP), "/", uri.Segments[3]); }
            else { repo_url = String.Concat(uri.Segments[1].TrimEnd(PATH_SEP), "/", uri.Segments[2]); }

            string result = url;
            result = String.Concat("https://api.github.com/repos/", repo_url).TrimEnd(PATH_SEP);
            return result;
        }

        private static string Format_As_Raw_GitHub_Url(string url)
        {
            var uri = new Uri(url);
            if (!host_is_github(uri.Host)) return uri.AbsolutePath;
            /*
            var reg = new Regex(@"^(/\w+/\w+)/.+$");
            Match match = reg.Match(uri.AbsolutePath);

            string result = url;
            if (match.Success) result = String.Concat("https://raw.github.com/", match.Groups[1].Value.TrimStart(new char[] {  '\\', '/'}));
            return result;
            */
            string result = String.Concat("https://raw.github.com/", uri.PathAndQuery);
            //PLog.Info("Format_As_Raw_GitHub_Url:  {0}", result);
            return result;
        }
        
        private static JSONArray Cache_Git_Repo(string repo_url)
        {
            //EXAMPLE:  https://api.github.com/repos/dsisco11/SR_Plugin_Loader/git/trees/master?recursive=1
            string url = String.Format("{0}/git/trees/master?recursive=1", repo_url.TrimEnd(new char[] { '\\', '/' }));
            string jsonStr = null;

            if (!remote_file_cache.ContainsKey(url))
            {
                // Fetch repo information
                //jsonStr = webClient.DownloadString(url);
                jsonStr = GetString(url);
                if (jsonStr == null || jsonStr.Length <= 0) return null;

                remote_file_cache.Add(url, ENCODING.GetBytes(jsonStr));
                SLog.Debug("Cached repository: {0}", url);
            }
            else jsonStr = ENCODING.GetString(remote_file_cache[url]);
            
            // Parse the json response from GitHub
            var git = SimpleJSON.JSON.Parse(jsonStr);
            var tree = git["tree"].AsArray;

            return tree;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="repo_url"></param>
        /// <returns>JSONArray holding all of the repository master branch entries</returns>
        private static IEnumerator Cache_Git_Repo_Async(string repo_url)
        {
            //EXAMPLE:  https://api.github.com/repos/dsisco11/SR_Plugin_Loader/git/trees/master?recursive=1
            string ghurl = Extract_Repository_URL_From_Github_URL(repo_url.TrimEnd(new char[] { '\\', '/' }));
            string url = String.Format("{0}/git/trees/master?recursive=1", ghurl);

            byte[] buf = null;
            string jsonStr = null;

            if (!remote_file_cache.ContainsKey(url))
            {
                // Fetch repo information
                //jsonStr = webClient.DownloadString(url);
                IEnumerator iter = GetAsync(url);
                while (iter.MoveNext()) yield return null;

                if(iter.Current == null)
                {
                    yield return null;
                    yield break;
                }

                buf = iter.Current as byte[];
                if (buf == null || buf.Length <= 0)
                {
                    yield return null;
                    yield break;
                }

                jsonStr = ENCODING.GetString(buf);
                if (jsonStr == null || jsonStr.Length <= 0)
                {
                    yield return null;
                    yield break;
                }

                //Check again just to make sure, because async methods could screw us up here.
                if(!remote_file_cache.ContainsKey(url)) remote_file_cache.Add(url, ENCODING.GetBytes(jsonStr));
                remote_file_cache[url] = ENCODING.GetBytes(jsonStr);

                SLog.Debug("Cached repository: {0}", repo_url);
            }
            else
            {
                jsonStr = ENCODING.GetString(remote_file_cache[url]);
                //PLog.Info("CACHE: {0}", jsonStr);
                //DebugHud.Log(remote_file_cache.ToLogString());
            }

            // Parse the json response from GitHub
            var git = SimpleJSON.JSON.Parse(jsonStr);
            var tree = git["tree"].AsArray;
                
            yield return tree;
            yield break;
        }

        public static JSONArray Get_Repo(string url)
        {
            string repo_url = Extract_Repository_URL_From_Github_URL(url);
            return Cache_Git_Repo(repo_url);
        }

        /// <param name="repo_url">The Raw CName GitHub repository url eg: https://raw.github.com/dsisco11/SR_Plugin_Loader/master/</param>
        /// <param name="folder"></param>
        public static List<UpdateFile> Get_Repo_Folder_Files(string repo_url, string folder)
        {
            string fldr = folder.Trim(new char[] { '/', '\\' });
            JSONArray repo = Get_Repo(repo_url);
            List<UpdateFile> list = new List<UpdateFile>();
            string raw_url = Format_As_Raw_GitHub_Url(repo_url).TrimEnd(new char[] { '\\', '/' });
            // Find the plugin loaders DLL installation file
            foreach (JSONNode file in repo)
            {
                string path = file["path"];
                //string url = file["url"];
                string url = String.Format("{0}/{1}", repo_url.TrimEnd(new char[] { '/', '\\' }), path.TrimStart(new char[] { '/', '\\' }));
                string dir = Path.GetDirectoryName(path);
                int size = string.IsNullOrEmpty(file["size"]) ? 0 : int.Parse(file["size"]);

                if (String.Compare("blob", file["type"].Value.ToLower()) != 0) continue;
                if (String.Compare(dir, fldr) == 0)
                {
                    //PLog.Info("{0}  |  URL: {1}", path, url);
                    list.Add(new UpdateFile(Path.Combine(repo_url, path), path, url, size));
                }
            }

            return list;
        }

        /// <param name="repo_url">The Raw CName GitHub repository url eg: https://raw.github.com/dsisco11/SR_Plugin_Loader/master/</param>
        /// <param name="folder"></param>
        /// <returns>List<GitFile></returns>
        public static IEnumerator Get_Repo_Folder_Files_Async(string repo_url, string folder)
        {
            string fldr = folder.Trim(new char[] { '/', '\\' });

            var iter = Cache_Git_Repo_Async(repo_url);
            while (iter.MoveNext()) yield return null;

            JSONArray repo = iter.Current as JSONArray;
            List<UpdateFile> list = new List<UpdateFile>();
            string raw_url = Format_As_Raw_GitHub_Url(repo_url).TrimEnd(new char[] { '\\', '/' });
            // Find the plugin loaders DLL installation file
            foreach (JSONNode file in repo)
            {
                string path = file["path"];
                //string url = file["url"];
                string url = String.Format("{0}/{1}", repo_url.TrimEnd(new char[] { '/', '\\' }), path.TrimStart(new char[] { '/', '\\' }));
                string dir = Path.GetDirectoryName(path);
                int size = string.IsNullOrEmpty(file["size"]) ? 0 : int.Parse(file["size"]);

                if (String.Compare("blob", file["type"].Value.ToLower()) != 0) continue;
                if (String.Compare(dir, fldr) == 0)
                {
                    //PLog.Info("{0}  |  URL: {1}", path, url);
                    list.Add(new UpdateFile(Path.Combine(repo_url, path), path, url, size));
                }
            }

            yield return list;
            yield break;
        }

        public static string Get_Repo_SHA(string repo_url)
        {
            //EXAMPLE:  https://api.github.com/repos/dsisco11/SR_Plugin_Loader/git/trees/master?recursive=1
            string url = String.Format("{0}/git/trees/master?recursive=1", repo_url.TrimEnd(new char[] { '\\', '/' }));

            if (!remote_file_cache.ContainsKey(url))
            {
                Cache_Git_Repo(repo_url);
            }

            string jsonStr = ENCODING.GetString(remote_file_cache[url]);
            // Parse the cached json response from GitHub
            var git = SimpleJSON.JSON.Parse(jsonStr);

            return git["sha"].Value;
        }

        #region Update Status

        private static void Reset_Tracker_For_Repo(string repo_url)
        {
            string rSHA = Get_Repo_SHA(repo_url);
            JSONClass nr = new JSONClass();
            nr["sha"] = rSHA;
            Tracker[repo_url] = nr;
            SLog.Debug("Reset tracker for repo. SHA: {0} | URL: {1}", rSHA, repo_url);
            Tracker.Save();
        }

        private static void Cache_Result(string remote_path, string local_path, FILE_UPDATE_STATUS val)
        {
            string repo_url = Extract_Repository_URL_From_Github_URL(remote_path);
            string rSHA = Get_Repo_SHA(repo_url);

            // Check and see if the current repository hash matches the one we were tracking for this url.
            JSONNode node = Tracker[repo_url];
            if (node == null) Reset_Tracker_For_Repo(repo_url);

            JSONClass repo = (JSONClass)node;
            string last_SHA = repo["sha"].Value;
            if (last_SHA == null || String.Compare(last_SHA, rSHA) != 0) Reset_Tracker_For_Repo(repo_url);

            // Get the hash for the file we're checking on.
            string cSHA = Util.Git_File_Sha1_Hash(local_path);            
            repo[cSHA] = new JSONData((int)val);
            Tracker.Save();
        }

        private static FILE_UPDATE_STATUS? Get_Cached_Result(string remote_path, string local_path)
        {
            string repo_url = Extract_Repository_URL_From_Github_URL(remote_path);
            string rSHA = Get_Repo_SHA(repo_url);

            // Check and see if the current repository hash matches the one we were tracking for this url.
            JSONNode tval = Tracker[repo_url];
            if (tval == null)
            {
                //Looks like we don't have a cached value for anything in this repo.
                // So let's create an instance for it and then return null.
                Reset_Tracker_For_Repo(repo_url);
                return null;
            }
            JSONClass repo = (JSONClass)tval;
            
            string last_SHA = repo["sha"].Value;
            if (last_SHA == null)
            {
                // For some reason we don't have a value for the repo sha. so clear the repo data, set the hash and return null.
                Reset_Tracker_For_Repo(repo_url);
                return null;
            }

            if (String.Compare(last_SHA, rSHA) != 0)
            {
                // Whelp looks like the repo has changed since we last started caching stuff. clear all of our, now old, data for it.
                Reset_Tracker_For_Repo(repo_url);
                return null;
            }

            // Get the hash for the file we're checking on.
            string cSHA = Util.Git_File_Sha1_Hash(local_path);

            // See if we have a stored result for a file with this hash
            if (repo[cSHA] == null) return null;

            // we do, return it!
            FILE_UPDATE_STATUS last = (FILE_UPDATE_STATUS)repo[cSHA].AsInt;
            return last;
        }

        public override FILE_UPDATE_STATUS Get_Update_Status(string remote_path, string local_file)
        {
            if (!File.Exists(local_file)) return FILE_UPDATE_STATUS.OUT_OF_DATE;

            string repo_url = Extract_Repository_URL_From_Github_URL(remote_path);
            string remote_file = Extract_File_Path_From_Github_URL(remote_path);

            // Time to check with GitHub and see if there is a newer version of the plugin loader out!
            try
            {
                JSONArray repo = Cache_Git_Repo(repo_url);
                // Go ahead and get the hash for the file we're checking on.
                string cSHA = Util.Git_File_Sha1_Hash(local_file);
                // Let's make sure we didn't already check on this same file in the past.
                FILE_UPDATE_STATUS? lastResult = Get_Cached_Result(remote_path, local_file);

                //if we DID cache the result from a past check against this file then return it here and don't waste time.
                if (lastResult.HasValue)
                {
                    SLog.Debug("Returning Cached Result: {2}  |  \"{0}\"  |  SHA({1})", local_file, cSHA, Enum.GetName(typeof(FILE_UPDATE_STATUS), lastResult.Value));
                    return lastResult.Value;
                }


                if (repo == null)
                {
                    SLog.Info("[AutoUpdater] Unable to cache git repository!");
                    return FILE_UPDATE_STATUS.ERROR;
                }

                // Find the plugin loaders DLL installation file
                foreach (JSONNode file in repo)
                {
                    if (String.Compare(file["path"], remote_file) == 0)
                    {
                        // Compare the SHA1 hash for the dll on GitHub to the hash for the one currently installed
                        string nSHA = file["sha"];
                        //PLog.Info("nSHA({0})  cSHA({1})  local_file: {2}", nSHA, cSHA, local_file);

                        if (String.Compare(nSHA, cSHA) != 0)
                        {
                            // ok they don't match, now let's just make double sure that it isn't because we are using an unreleased developer version
                            // First find the url for the file on GitHub
                            string tmpurl = file["url"];
                            // now we want to replace it's hash with ours and check if it exists!
                            tmpurl = tmpurl.Replace(nSHA, cSHA);

                            // Check if the file for the currently installed loaders sha1 hash exists.
                            bool exist = false;
                            try
                            {
                                exist = Query_Remote_File_Exists(tmpurl);
                                //PLog.Info("Query_Remote_File_Exists: {0}  = {1}", tmpurl, (exist ? "TRUE" : "FALSE"));
                            }
                            catch (WebException wex)
                            {
                                if (wex.Status == WebExceptionStatus.ProtocolError)
                                {
                                    var response = wex.Response as HttpWebResponse;
                                    if (response != null)
                                    {
                                        if (response.StatusCode == HttpStatusCode.NotFound)// A file for this hash does not exhist on the github repo. So this must be a Dev version.
                                        {
                                            exist = false;
                                        }
                                    }
                                }
                            }

                            if (!exist)
                            {
                                //PLog.Info("[Updater] Dev file: {0}", Path.GetFileName(local_file));
                                Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.DEV_FILE);
                                return FILE_UPDATE_STATUS.DEV_FILE;
                            }

                            //PLog.Info("[Updater] Outdated file: {0}", Path.GetFileName(local_file));
                            Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.OUT_OF_DATE);
                            return FILE_UPDATE_STATUS.OUT_OF_DATE;
                        }
                        else
                        {
                            Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.UP_TO_DATE);
                            return FILE_UPDATE_STATUS.UP_TO_DATE;
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                SLog.Error(wex);
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
            }

            SLog.Debug("[Git_Updater] Unable to find file in repository: {0}", remote_file);
            return FILE_UPDATE_STATUS.NOT_FOUND;//no update
        }

        public IEnumerator Get_Update_Status_Async(string remote_path, string local_file)
        {
            if (!File.Exists(local_file))
            {
                yield return FILE_UPDATE_STATUS.OUT_OF_DATE;
                yield break;// ensure we stop here
            }

            string repo_url = Extract_Repository_URL_From_Github_URL(remote_path);
            string remote_file = Extract_File_Path_From_Github_URL(remote_path);

            // Time to check with GitHub and see if there is a newer version of the plugin loader out!
            JSONArray repo = Cache_Git_Repo(repo_url);
            // Go ahead and get the hash for the file we're checking on.
            string cSHA = Util.Git_File_Sha1_Hash(local_file);
            // Let's make sure we didn't already check on this same file in the past.
            FILE_UPDATE_STATUS? lastResult = Get_Cached_Result(remote_path, local_file);

            //if we DID cache the result from a past check against this file then return it here and don't waste time.
            if (lastResult.HasValue)
            {
                //SLog.Debug("Cached {2}  |  \"{0}\"  |  SHA({1})", local_file, cSHA, Enum.GetName(typeof(FILE_UPDATE_STATUS), lastResult.Value));
                yield return lastResult.Value;
                yield break;// ensure we stop here
            }


            if (repo == null)
            {
                SLog.Info("[AutoUpdater] Unable to cache git repository!");
                yield return FILE_UPDATE_STATUS.ERROR;
                yield break;// ensure we stop here
            }

            // Find the plugin loaders DLL installation file
            foreach (JSONNode file in repo)
            {
                if (String.Compare(file["path"], remote_file) == 0)
                {
                    // Compare the SHA1 hash for the dll on GitHub to the hash for the one currently installed
                    string nSHA = file["sha"];
                    //PLog.Info("nSHA({0})  cSHA({1})  local_file: {2}", nSHA, cSHA, local_file);

                    if (String.Compare(nSHA, cSHA) != 0)
                    {
                        // ok they don't match, now let's just make double sure that it isn't because we are using an unreleased developer version
                        // First find the url for the file on GitHub
                        string tmpurl = file["url"];
                        // now we want to replace it's hash with ours and check if it exists!
                        tmpurl = tmpurl.Replace(nSHA, cSHA);

                        // Check if the file for the currently installed loaders sha1 hash exists.
                        bool exist = false;
                        var it = Query_Remote_File_Exists_Async(tmpurl);
                        bool fail = false;
                        do
                        {
                            yield return null;
                            
                            try
                            {
                                if (!it.MoveNext()) fail = true;//stop
                            }
                            catch (WebException wex)
                            {
                                fail = true;//stop
                                if (wex.Status == WebExceptionStatus.ProtocolError)
                                {
                                    var response = wex.Response as HttpWebResponse;
                                    if (response != null)
                                    {
                                        if (response.StatusCode == HttpStatusCode.NotFound)// A file for this hash does not exhist on the github repo. So this must be a Dev version.
                                        {
                                            exist = false;
                                        }
                                    }
                                }
                            }
                        } while (!fail);

                        if (!exist)
                        {
                            //PLog.Info("[Updater] Dev file: {0}", Path.GetFileName(local_file));
                            Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.DEV_FILE);
                            yield return FILE_UPDATE_STATUS.DEV_FILE;
                            yield break;// ensure we stop here
                        }

                        //PLog.Info("[Updater] Outdated file: {0}", Path.GetFileName(local_file));
                        Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.OUT_OF_DATE);
                        yield return FILE_UPDATE_STATUS.OUT_OF_DATE;
                        yield break;// ensure we stop here
                    }
                    else
                    {
                        Cache_Result(remote_path, local_file, FILE_UPDATE_STATUS.UP_TO_DATE);
                        yield return FILE_UPDATE_STATUS.UP_TO_DATE;
                        yield break;// ensure we stop here
                    }
                }
            }

            SLog.Warn("[Git_Updater] Unable to find file in repository: {0}", remote_file);
            yield return FILE_UPDATE_STATUS.NOT_FOUND;//no update
            yield break;// ensure we stop here
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns>FileStream for the requested file.</returns>
        public IEnumerator Cache_And_Open_File_Async(string url)
        {
            string repo_url = Extract_Repository_URL_From_Github_URL(url);
            var iter = Cache_Git_Repo_Async(repo_url);
            while (iter.MoveNext()) yield return null;

            JSONArray repo = iter.Current as JSONArray;

            string local_file = String.Format("{0}\\{1}", UnityEngine.Application.dataPath, Path.GetFileName(url));            
            var iter1 = Get_Update_Status_Async(url, local_file);
            while (iter1.MoveNext()) yield return null;

            var update_status = (FILE_UPDATE_STATUS)iter1.Current;
            if (update_status == FILE_UPDATE_STATUS.OUT_OF_DATE)
            {
                SLog.Debug("Git_Updater.Cache_And_Open_File(): Downloading: {0}  |  File: {1}", url, Path.GetFileName(local_file));
                var it = DownloadAsync(url, local_file);
                while (it.MoveNext()) yield return null;
            }
            
            yield return File.OpenRead(local_file);
            yield break;
        }
        

        /// <summary>
        /// Checks if a specified file exists in the project GIT repository.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Query_Remote_File_Exists(string url)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            if(request == null)
            {
                SLog.Info("Unable to create an instance of HttpWebRequest!");
                return false;
            }
            request.UserAgent = USER_AGENT;
            request.Method = "HEAD";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    var wres = wex.Response as HttpWebResponse;
                    if (wres != null)
                    {
                        if (wres.StatusCode == HttpStatusCode.NotFound)// A file for this hash does not exist on the github repo. So this must be a Dev version.
                        {
                            return false;
                        }
                    }
                }

                //DebugHud.Log(wex);
                return false;
            }
            finally
            {
                if (response != null) response.Close();
            }

            return true;
        }
        
        /// <summary>
        /// Checks if a specified file exists in the project GIT repository.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IEnumerator Query_Remote_File_Exists_Async(string url)
        {
            HttpWebResponse response = null;
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            if (webRequest == null)
            {
                SLog.Info("Unable to create an instance of HttpWebRequest!");
                yield return false;
            }

            webRequest.UserAgent = USER_AGENT;
            webRequest.Method = "HEAD";

            WebAsync webAsync = new WebAsync();
            IEnumerator e = webAsync.GetResponse(webRequest);
            if (e == null)
            {
                SLog.Info("Updater_Base.Get() Enumerator is NULL!");
                yield return false;
                yield break;
            }

            bool failed = false;
            bool done = false;

            do// wait for response to arrive
            {
                try
                {
                    done = !e.MoveNext();
                }
                catch (Exception ex)
                {
                    SLog.Error(ex);
                    failed = true;
                }

                yield return null;
            }
            while (!done);

            while (!webAsync.isResponseCompleted) yield return null;// double check for clarity & safety

            if (!failed)
            {
                try
                {
                    RequestState result = webAsync.requestState;
                    response = (HttpWebResponse)result.webResponse;
                }
                catch (WebException wex)
                {
                    if (wex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var wres = wex.Response as HttpWebResponse;
                        if (wres != null)
                        {
                            if (wres.StatusCode == HttpStatusCode.NotFound)// A file for this hash does not exist on the github repo. So this must be a Dev version.
                            {
                                failed = true;
                            }
                        }
                    }

                    failed = true;
                }
                finally
                {
                    if (response != null) response.Close();
                }
            }

            yield return !failed;
            yield break;
        }
    }

}
