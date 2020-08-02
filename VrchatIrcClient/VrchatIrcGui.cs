using Il2CppSystem;
using UIExpansionKit;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib;
using Action = System.Action;
using Delegate = System.Delegate;
using Int32 = System.Int32;
using IntPtr = System.IntPtr;
using NotImplementedException = System.NotImplementedException;

namespace VrchatIrcClient
{
    public class VrchatIrcGui : MonoBehaviour
    {
        public static Action ClearChatFunction;
        public static Action SendChatText;
        public InputField ChatInput;
        public Button ChatEntry;
        public Text ChatOutput;
        public Button ClearChat;
        public GameObject mymenu;
        public Delegate ReferencedDelegate;
        public IntPtr MethodInfo;
        public Il2CppSystem.Collections.Generic.List<MonoBehaviour> AntiGcList;
        public GridLayoutGroup Ass;
        public void IntializeGui()
        {
            mymenu = new GameObject((Il2CppSystem.String)"AAAA");
            var gridme = mymenu.AddComponent<GridLayoutGroup>();
            var textinput = mymenu.AddComponent<InputField>();
            textinput.readOnly = false;
            //ClearChatFunction = Class1.myistance.ClearChat;
            //SendChatText = SendTextChatFunct;
            //ChatInput = mymenu.AddComponent<InputField>().TryCast<InputField>();
            //ChatInput.characterLimit = int.MaxValue;
            //ChatInput.keyboardType = TouchScreenKeyboardType.Default;
            //ChatEntry = mymenu.AddComponent<Button>().TryCast<Button>();
            //ChatOutput = mymenu.AddComponent<Text>().TryCast<Text>();
            //ClearChat = mymenu.AddComponent<Button>().TryCast<Button>();
            //ChatEntry.onClick = new Button.ButtonClickedEvent(((Il2CppSystem.Action)SendChatText).Pointer );
            //ClearChat.onClick = new Button.ButtonClickedEvent(((Il2CppSystem.Action)ClearChatFunction).Pointer );
            ExpansionKitApi.RegisterCustomMenuButton(ExpandedMenu.SocialMenu,mymenu);
        }
        public VrchatIrcGui(IntPtr obj0) : base(obj0) {
            AntiGcList = new Il2CppSystem.Collections.Generic.List<MonoBehaviour>(1);
            AntiGcList.Add(this);
        }

        public void SendTextChatFunct()
        {
            var text = ChatInput.text;
            Class1.myistance.SendChat(text);
        }

        public void Update()
        {
            //ChatOutput.text = (Il2CppSystem.String)Class1.myistance.GetOutput();
        }
    }
}