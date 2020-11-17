using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using VrchatIrcClient;

[assembly: MelonInfo(typeof(Class1), "IrcClient", "1.0", "Noah")]
[assembly: MelonGame(null, null)]
namespace VrchatIrcClient
{
    public class Class1 : MelonMod
    {
        public static IrcInstance myistance;
        public static IrcConsoleGui mygui;
        public static string settingsCategory = "IrcClientForUnityGames";
        public static readonly string[] IrcMenuNames = {"IrcUserName", "IrcRealName","IrcNickName","IrcDefaultServer"};
        public static readonly string[] IrcMenuDisplay = {"Username to use for Irc","Real Name to use for Irc","Nick Name to use for Irc","Default server to use <servername>:<port>:<useSSL>"};
        public void RegisterModPrefs()
        {
            MelonPrefs.RegisterCategory(settingsCategory, "Irc Chat settings");
            MelonPrefs.RegisterString(settingsCategory,IrcMenuNames[0],"VrchatUser",IrcMenuDisplay[0]);
            MelonPrefs.RegisterString(settingsCategory,IrcMenuNames[1],"Vrchat User",IrcMenuDisplay[1]);
            MelonPrefs.RegisterString(settingsCategory,IrcMenuNames[2],"VrchatUser",IrcMenuDisplay[2]);
            MelonPrefs.RegisterString(settingsCategory,IrcMenuNames[3],"chat.freenode.net:6697:true",IrcMenuDisplay[3]);

        }

        public static string[] GetModData()
        {
            var username = MelonPrefs.GetString(settingsCategory, IrcMenuNames[0]);
            var realname = MelonPrefs.GetString(settingsCategory, IrcMenuNames[1]);
            var nickname = MelonPrefs.GetString(settingsCategory, IrcMenuNames[2]);
            var defaultServer = MelonPrefs.GetString(settingsCategory, IrcMenuNames[3]);
            return new[] {username, realname, nickname,defaultServer};
        }
        public override void OnApplicationStart()
        {
            RegisterModPrefs();
            myistance = new IrcInstance();
            ClassInjector.RegisterTypeInIl2Cpp<IrcConsoleGui>();
        }
        public override void OnLevelWasLoaded(int level)
        {
            var myobject = new GameObject().TryCast<GameObject>();
            mygui = myobject.AddComponent<IrcConsoleGui>();
        }

        public override void OnUpdate()
        {
        }
    }
    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            CosturaUtility.Initialize();
        }
    }
}