using System;
using System.Linq;
using Harmony;
using IrcDotNet;
using MelonLoader;

namespace VrchatIrcClient
{
    public class IrcInstance
    {
        private StandardIrcClient myclient;
        private string OutputLog = "";

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
            var userdata = Class1.GetModData();
            IrcUserRegistrationInfo myreg;
            if (commandstring == "#/connect default")
            {
                myclient = new StandardIrcClient();
                setup_events();
                setup_settings();
                var server = userdata[3].Split(':');
                myreg = new IrcUserRegistrationInfo()
                {
                    NickName = userdata[2],
                    UserName = userdata[0],
                    RealName = userdata[1]
                };
                myclient.Connect(server[0], int.Parse((Il2CppSystem.String) server[1]), bool.Parse(server[2]), myreg);
                return;
            }
            if (!commandstring.StartsWith("#/connect")) return;
            var hostargs = commandstring.Replace("#/connect", "").Trim().Split(' ');
            myclient = new StandardIrcClient();
            setup_events();
            setup_settings();

            myreg = new IrcUserRegistrationInfo()
            {
                NickName = userdata[2],
                UserName = userdata[0],
                RealName = userdata[1]
            };
            myclient.Connect(hostargs[0], int.Parse((Il2CppSystem.String) hostargs[1]), false, myreg);
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

        public void ClearChat()
        {
            OutputLog = "";
        }
    }
}