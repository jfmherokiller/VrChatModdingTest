using System;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace VrchatIrcClient
{
    public class IrcConsoleGui : MonoBehaviour
    {
        private  bool ShowJsConsole;
        public string CodeString = "";
        private Rect windowRect = new Rect(20, 20, 1200, 500);
        private IrcInstance CodeInstance;
        public static Action<int> windowfunction;
        public Delegate ReferencedDelegate;
        public IntPtr MethodInfo;
        public Il2CppSystem.Collections.Generic.List<MonoBehaviour> AntiGcList;
        private Vector2 CodeScroll;
        private Vector2 OutputScroll;
        public IrcConsoleGui(IntPtr obj0) : base(obj0) {
            AntiGcList = new Il2CppSystem.Collections.Generic.List<MonoBehaviour>(1);
            AntiGcList.Add(this);
            windowfunction = ConsoleWindowDisplay;
            CodeInstance = new IrcInstance();
        }
        public IrcConsoleGui(Delegate referencedDelegate, IntPtr methodInfo) : base(ClassInjector.DerivedConstructorPointer<IrcConsoleGui>()) {
            ClassInjector.DerivedConstructorBody(this);

            ReferencedDelegate = referencedDelegate;
            MethodInfo = methodInfo;
        }
        ~IrcConsoleGui() {
            Marshal.FreeHGlobal(MethodInfo);
            MethodInfo = IntPtr.Zero;
            ReferencedDelegate = null;
            AntiGcList.Remove(this);
            AntiGcList = null;
        }

        public void OnGUI()
        {
            if (!ShowJsConsole) return;
            // Make a background box
            
            windowRect = GUI.ModalWindow(0, windowRect, windowfunction, "Irc Console");
        }
        public void ConsoleWindowDisplay(int windowID)
        {
            Cursor.lockState = CursorLockMode.None;
            GUILayout.BeginVertical(null);
            GUILayout.Label("Irc Command Below",null);
            CodeScroll = GUILayout.BeginScrollView(CodeScroll,GUIStyle.none,null);
            CodeString = GUILayout.TextArea(CodeString,null);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Run Code",null))
            {
                Class1.myistance.SendChat(CodeString);
            }
            GUILayout.Label("Code Results Below",null);
            OutputScroll = GUILayout.BeginScrollView(OutputScroll,GUIStyle.none,null);
            GUILayout.TextArea(Class1.myistance.GetOutput(),null);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close Console",null))
            {
                ShowJsConsole = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                OpenConsole();
            }
        }
        public void OpenConsole()
        {
            ShowJsConsole = true;
        }
        
    }
}