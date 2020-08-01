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
        private string OutputLog;
        public IrcInstance()
        {
            myclient = new StandardIrcClient();
        }
        public void SendChat(string codeString)
        {
            if (codeString.StartsWith("#/"))
            {
                HandleCommands(codeString);
            }
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
                    Description = "TEST",
                    Distribution = "test",
                    NickName = "vrc_TEST",
                } );
                myclient.ListChannels();
                OutputLog = myclient.Channels.Select(item => item.Name).Join();
            }
            
            if (commandstring.StartsWith("#/joinc"))
            {
                var channelname = commandstring.Replace("#/joinc","").Trim();
                myclient.Channels.First(item => item.Name == channelname).MessageReceived += RecieveMessage;
                myclient.Channels.Join(channelname);
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