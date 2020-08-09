using System;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnityEngine;
using UnhollowerRuntimeLib;
namespace PulsarCRepl
{
    public class JintConsoleGui : MonoBehaviour
    {
        private  bool ShowJsConsole;
        public string CodeString = "";
        private Rect windowRect = new Rect(20, 20, 1200, 500);
        public static Action<int> windowfunction;
        public Delegate ReferencedDelegate;
        public IntPtr MethodInfo;
        public Il2CppSystem.Collections.Generic.List<MonoBehaviour> AntiGcList;
        private Vector2 CodeScroll;
        private Vector2 OutputScroll;
        private JintInstance myinstance;
        public JintConsoleGui(IntPtr obj0) : base(obj0) {
            AntiGcList = new Il2CppSystem.Collections.Generic.List<MonoBehaviour>(1);
            AntiGcList.Add(this);
            windowfunction = ConsoleWindowDisplay;
            myinstance = PulsarCRepMod.MakeNewJintInstance();
        }
        public JintConsoleGui(Delegate referencedDelegate, IntPtr methodInfo) : base(ClassInjector.DerivedConstructorPointer<JintConsoleGui>()) {
            ClassInjector.DerivedConstructorBody(this);

            ReferencedDelegate = referencedDelegate;
            MethodInfo = methodInfo;
        }
        ~JintConsoleGui() {
            Marshal.FreeHGlobal(MethodInfo);
            MethodInfo = IntPtr.Zero;
            ReferencedDelegate = null;
            AntiGcList.Remove(this);
            AntiGcList = null;
            myinstance = null;
        }

        public void OnGUI()
        {
            if (!ShowJsConsole) return;
            // Make a background box
            
            windowRect = GUI.ModalWindow(0, windowRect, windowfunction, "Javascript Console");
        }
        public void ConsoleWindowDisplay(int windowID)
        {
            Cursor.lockState = CursorLockMode.None;
            GUILayout.BeginVertical(null);
            GUILayout.Label("Insert Javascript Code below",null);
            CodeScroll = GUILayout.BeginScrollView(CodeScroll,GUIStyle.none,null);
            CodeString = GUILayout.TextArea(CodeString,null);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Run Code",null))
            {
                myinstance.ExecuteCode(CodeString);
            }
            GUILayout.Label("Code Results Below",null);
            OutputScroll = GUILayout.BeginScrollView(OutputScroll,GUIStyle.none,null);
            GUILayout.TextArea(myinstance.GetOutput(),null);
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
            if (Input.GetKeyDown(KeyCode.Keypad1))
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