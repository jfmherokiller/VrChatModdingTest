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
            myclient = new StandardIrcClient();
            setup_events();
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
            myclient.MotdReceived += (sender, args) => { OutputLog += myclient.MessageOfTheDay;};
            myclient.ErrorMessageReceived += (sender, args) => { OutputLog += args.Message; };
            ///     Client information is accessible via <see cref="WelcomeMessage" />, <see cref="YourHostMessage" />,
            ///     <see cref="ServerCreatedMessage" />, <see cref="ServerName" />, <see cref="ServerVersion" />,
            ///     <see cref="ServerAvailableUserModes" />, and <see cref="ServerAvailableChannelModes" />.
            myclient.ClientInfoReceived += (sender, args) => { OutputLog += myclient.WelcomeMessage; };
            myclient.RawMessageReceived += (sender, args) => { OutputLog += args.RawContent; };
        }

        public string GetOutput()
        {
            return OutputLog;
        }

        public void HandleCommands(string commandstring)
        {
            if (commandstring.StartsWith("#/connect"))
            {
                var hostargs = commandstring.Replace("#/connect", "").Trim().Split(' ');
                MelonModLogger.Log(hostargs[1]);
                myclient.Connect(hostargs[0],int.Parse((Il2CppSystem.String)hostargs[1]),false,new IrcServiceRegistrationInfo()
                {
                    Description = "A Vrchat user",
                    Distribution = "test",
                    NickName = Player.prop_Player_0.prop_APIUser_0.displayName,
                } );
            }
            
            if (commandstring.StartsWith("#/joinc"))
            {
                var channelname = commandstring.Replace("#/joinc","").Trim();
                myclient.Channels.First(item => item.Name == channelname).MessageReceived += RecieveMessage;
                myclient.Channels.Join(channelname);
            }
            if (commandstring == ("#/quit"))
            {
                myclient.Disconnect();
            }
        }

        public void RecieveMessage(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel)sender;
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