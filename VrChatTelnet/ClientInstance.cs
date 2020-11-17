using System;
using System.Linq;
using System.Threading;
using MelonLoader;
using VtNetCore.VirtualTerminal;

namespace VrChatTelnet
{
    public class ClientInstance : IDisposable
    {
        CancellationTokenSource ctc;
        private TelnetClient telnetcli;
        private string OutputLog = "";
        private VirtualTerminalController vtController;

        public void setupConnection(string host, int port)
        {
            ctc = new CancellationTokenSource();
            //vtController = new VirtualTerminalController();
            
            telnetcli = new TelnetClient(host, port, TimeSpan.FromSeconds(0), ctc.Token);
            telnetcli.DataReceived += (sender, s) => HandleTerminalEscapes(s);
            //telnetcli.LineReceived += (sender, s) => OutputLog += s;
        }
        public ClientInstance()
        {
            
        }

        private void HandleTerminalEscapes(string escape)
        {
            //handle clear screen
            if (escape.Contains("\x1B[H"))
            {
                ClearChat();
            }
            else
            {
                OutputLog += escape;
            }
        }
        public async void Connect()
        {
            await telnetcli.Connect();
        }
        public void RecieveMessage(object sender, string e)
        {
            OutputLog += ($"{e}\n");
        }
        public string GetOutput()
        {
            return OutputLog;
        }

        public void ClearChat()
        {
            OutputLog = "";
        }

        public void SendChat(string codeString)
        {
            if (codeString.StartsWith("#/"))
            {
                HandleCommands(codeString);
            }
            else
            {
                telnetcli.Send(codeString);
            }
        }

        private void HandleCommands(string codeString)
        {
            if (codeString.StartsWith("#/joins"))
            {
                var ServerInfo= codeString.Replace("#/joins", "").Trim();
                var serverHost = ServerInfo.Split(':')[0].Trim();
                var serverPort = int.Parse(ServerInfo.Split(':')[1].Trim());
                setupConnection(serverHost,serverPort);
                Connect();
            }
        }

        public void Dispose()
        {
        }
    }
}