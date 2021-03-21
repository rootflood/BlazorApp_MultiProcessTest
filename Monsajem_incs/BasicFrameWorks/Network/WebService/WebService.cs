using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Monsajem_Incs.Net.Base.Socket;
using WebSocketSharp;
using System;
using static Monsajem_Incs.DelegateExtentions.Actions;

namespace Monsajem_Incs.Net.Web
{
    public class Server:
        Base.Service.Server<int>
    {
        public Server():base
            (new ServerSocket())
        { }

        private class Behavior : WebSocketBehavior
        {
            public WebSocket Socket;

            protected override void OnError(ErrorEventArgs e)
            {
                base.OnError(e);
#if DEBUG
                if (Socket.WhyDisconnect == null)
                    Socket.WhyDisconnect = "Closed By this side Because of error >> "+e.Message;
#endif
                base.Close();
                Socket.IsConnected = false;
            }

            protected override void OnClose(CloseEventArgs e)
            {
#if DEBUG
                if (Socket.WhyDisconnect == null)
                    Socket.WhyDisconnect = "Closed By Other side";
#endif
                base.OnClose(e);
                Socket.IsConnected = false;
            }

            public new void Send(byte[] Buffer)
            {
                base.Send(Buffer);
            }

            public new void Close(){
#if DEBUG
                if (Socket.WhyDisconnect == null)
                    Socket.WhyDisconnect = "Closed By this side";
#endif
                base.Close();
                Socket.IsConnected = false;
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Socket.Recived(e.RawData);
            }
        }
        private class WebSocket :
            Net.Base.Socket.ClientSocket<int>
        {
            public WebSocket(Behavior Client)
            {
                this.Client = Client;
                Client.Socket = this;
                this.IsConnected = true;
            }

            private Behavior Client;

            public new void Recived(byte[] Data) => base.Recived(Data);

            protected async override Task Inner_Send(byte[] Data)
            {
                Client.Send(Data);
            }

            protected override async Task Inner_Disconnect()
            {
                Client.Close();
            }
        }
        private class ServerSocket :
        Net.Base.Socket.ServerSocket<int>
        {
            public WebSocketSharp.Server.WebSocketServer wss;

            private WebSocket[] Accepted = new WebSocket[0];

            private Action OnAccept;

            protected override void OnBeginService(int Address)
            {
                wss = new WebSocketSharp.Server.WebSocketServer(System.Net.IPAddress.Any,Address);
                wss.AddWebSocketService<Behavior>("/Client",(c)=>
                {
                    lock (Accepted)
                        Insert(ref Accepted, new WebSocket(c), 0);
                    OnAccept?.Invoke();
                });
                wss.Start();
            }

            protected override ClientSocket<int> OnWaitForAccep()
            {
                if (Accepted.Length == 0)
                {
                    WaitForHandle(()=> ref OnAccept).Wait();
                }
                lock(Accepted)
                    return Pop(ref Accepted);
            }

            protected override void OnDisconnect()
            {
                wss.Stop();
            }
        }
    }
}
