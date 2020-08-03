using System;
using System.Collections;
using Il2CppSystem.Runtime.InteropServices;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using Xilium.CefGlue;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace RealBrowser
{
    //[DisallowMultipleComponent]
    //[RequireComponent(typeof(MeshRenderer))]
    public class OffscreenCEF : MonoBehaviour
    {
        //[Space]
        //[SerializeField]
        private Size windowSize = new Size(1280, 720);

        //[SerializeField]
        private string url = "http://www.google.com";

        //[Space]
        //[SerializeField]
        private bool hideScrollbars = false;

        private bool shouldQuit = false;
        private OffscreenCEFClient cefClient;

        public Texture2D BrowserTexture { get; private set; }
        public Delegate ReferencedDelegate;
        public IntPtr MethodInfo;
        public Il2CppSystem.Collections.Generic.List<MonoBehaviour> AntiGcList;
        public Task myfunct;
        public Action functionOpbject;
        private void Awake()
        {
            this.BrowserTexture =
                new Texture2D(this.windowSize.Width, this.windowSize.Height, TextureFormat.BGRA32, false).TryCast<Texture2D>();
            this.GetComponent<MeshRenderer>().TryCast<MeshRenderer>().material.mainTexture = this.BrowserTexture;
        }

        private void Start()
        {
            this.StartCef();
            DontDestroyOnLoad(this.gameObject.transform.root.gameObject);
        }

        private void OnDestroy()
        {
            this.Quit();
        }

        private void OnApplicationQuit()
        {
            this.Quit();
        }

        private void FixedUpdate()
        {
            CefRuntime.DoMessageLoopWork();
        }

        private void LateUpdate()
        {
            if (!this.shouldQuit)
            {
                this.cefClient.UpdateTexture(this.BrowserTexture);
            }
        }

        private void StartCef()
        {
            CefRuntime.Load();
            var cefMainArgs = new CefMainArgs(new string[] { });
            var cefApp = new OffscreenCEFClient.OffscreenCEFApp();

            // This is where the code path diverges for child processes.
            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
                MelonModLogger.Log("Could not start the secondary process.");

            var cefSettings = new CefSettings
            {
                MultiThreadedMessageLoop = false,
                SingleProcess = true,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
                WindowlessRenderingEnabled = true,
                NoSandbox = true,
                
            };

            // Start the browser process (a child process).
            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);

            // Instruct CEF to not render to a window.
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);

            // Settings for the browser window itself (e.g. enable JavaScript?).
            var cefBrowserSettings = new CefBrowserSettings()
            {
                BackgroundColor = new CefColor(255, 60, 85, 115),
                JavaScript = CefState.Enabled,
                JavaScriptAccessClipboard = CefState.Disabled,
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptDomPaste = CefState.Disabled,
                JavaScriptOpenWindows = CefState.Disabled,
                LocalStorage = CefState.Disabled
            };

            // Initialize some of the custom interactions with the browser process.
            this.cefClient = new OffscreenCEFClient(this.windowSize, this.hideScrollbars);

            // Start up the browser instance.
            CefBrowserHost.CreateBrowser(cefWindowInfo, this.cefClient, cefBrowserSettings,
                string.IsNullOrEmpty(this.url) ? "http://www.google.com" : this.url);
        }

        private void Quit()
        {
            this.shouldQuit = true;
            this.cefClient.Shutdown();
            CefRuntime.Shutdown();
        }
        public OffscreenCEF(IntPtr obj0) : base(obj0)
        {
            AntiGcList = new Il2CppSystem.Collections.Generic.List<MonoBehaviour>(1);
            AntiGcList.Add(this);
        }

        public OffscreenCEF(Delegate referencedDelegate, IntPtr methodInfo) : base(ClassInjector
            .DerivedConstructorPointer<OffscreenCEF>())
        {
            ClassInjector.DerivedConstructorBody(this);

            ReferencedDelegate = referencedDelegate;
            MethodInfo = methodInfo;
        }

        ~OffscreenCEF()
        {
            Marshal.FreeHGlobal(MethodInfo);
            MethodInfo = IntPtr.Zero;
            ReferencedDelegate = null;
            AntiGcList.Remove(this);
            AntiGcList = null;
        }
    }
}