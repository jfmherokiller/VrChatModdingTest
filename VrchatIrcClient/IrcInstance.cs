using System;
using System.Linq;
using Harmony;
using IrcDotNet;
using MelonLoader;
using VRC;

namespace VrchatIrcClient
{
    public class IrcInstance
    {
        private StandardIrcClient myclient;
        private string OutputLog;

        public IrcInstance()
        {
        }

        private void setup_settings()
        {
            myclient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
        }

        public void SendChat(string codeString)
        {
            if (codeString.StartsWith("#/"))
            {
                HandleCommands(codeString);
            }
            else
            {
                myclient.SendRawMessage(codeString);
            }
        }

        private void setup_events()
        {
            myclient.ChannelListReceived += (sender, args) =>
            {
                OutputLog += args.Channels.Select(item => item.Name).Join();
            };
            myclient.NetworkInformationReceived += (sender, args) => { OutputLog += args.Comment; };
            myclient.MotdReceived += (sender, args) => { OutputLog += myclient.MessageOfTheDay; };
            myclient.ErrorMessageReceived += (sender, args) => { OutputLog += args.Message; };
            myclient.ConnectFailed += (sender, args) => { OutputLog += args.Error.Message; };
            myclient.Error += (sender, args) => { OutputLog += args.Error.Message; };
            myclient.ClientInfoReceived += (sender, args) =>
            {
                OutputLog +=
                    $"{myclient.WelcomeMessage}\n {myclient.YourHostMessage}\n {myclient.ServerCreatedMessage}\n {myclient.ServerName}\n {myclient.ServerVersion}\n {myclient.ServerAvailableUserModes}\n {myclient.ServerAvailableChannelModes}";
            };
        }

        public string GetOutput()
        {
            return OutputLog;
        }

        public void HandleCommands(string commandstring)
        {
            ConnectMethod(commandstring);

            if (commandstring.StartsWith("#/joinc"))
            {
                var channelname = commandstring.Replace("#/joinc", "").Trim();
                var selected = myclient.Channels.First(item => item.Name == channelname);
                selected.MessageReceived += RecieveMessage;
                selected.NoticeReceived += (sender, args) => { OutputLog += args.GetText(); };
                myclient.Channels.Join(channelname);
            }

            if (commandstring == ("#/quit"))
            {
                myclient.Quit("exiting");
                myclient.Dispose();
            }
        }

        private void ConnectMethod(string commandstring)
        {
            if (commandstring.StartsWith("#/connect"))
            {
                var hostargs = commandstring.Replace("#/connect", "").Trim().Split(' ');
                myclient = new StandardIrcClient();
                setup_events();
                setup_settings();
                var myreg = new IrcUserRegistrationInfo()
                {
                    NickName = Player.prop_Player_0.prop_APIUser_0.displayName.Replace(' ', '_'),
                    UserName = Player.prop_Player_0.prop_APIUser_0.displayName.Substring(0, 8),
                    RealName = Player.prop_Player_0.prop_APIUser_0.displayName.Replace(' ', '_')
                };
                myclient.Connect(hostargs[0], int.Parse((Il2CppSystem.String) hostargs[1]), false, myreg);
            }
        }

        public void RecieveMessage(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel) sender;
            if (e.Source is IrcUser)
            {
                // Read message.
                OutputLog += ($"[{channel.Name}]({e.Source.Name}): {e.Text}.\n");
            }
            else
            {
                OutputLog += ($"[{channel.Name}]({e.Source.Name}) Message: {e.Text}.\n");
            }
        }
    }
}