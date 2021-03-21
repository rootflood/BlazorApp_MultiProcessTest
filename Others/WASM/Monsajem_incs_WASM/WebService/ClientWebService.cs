using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.Net.Web
{
    public class EndPoint
    {
        public string IpAddress;
        public int Port;
    }
    public class Client : Net.Base.Service.Client<EndPoint>
    {
        public Client() :
            base(new ClientSocket())
        { }
        private class ClientSocket : Net.Base.Socket.ClientSocket<EndPoint>
        {
            public WebAssembly.Net.WebSockets.ClientWebSocket Socket;

            protected async override Task Inner_Connect(EndPoint Address)
            {
                Socket = new WebAssembly.Net.WebSockets.ClientWebSocket();
                await Socket.ConnectAsync(
                        new Uri("ws://" + Address.IpAddress + ":" + Address.Port.ToString() + "/Client/"), 
                        CancellationToken.None);
            }

            public async override Task<int> Recive(byte[] Buffer)
            {
                var Array = new ArraySegment<byte>(Buffer);
                var Result = (await Socket.ReceiveAsync(Array, CancellationToken.None)).Count;
                var ReciverArray = Array.Array;
                ReciverArray.CopyTo(Buffer, 0);
                return Result;
            }

            protected override async Task Inner_Disconnect()
            {
                if (Socket.State != WebSocketState.Open)
                    return;
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,"", CancellationToken.None);
            }

            protected override async Task Inner_Send(byte[] Data)
            {
                var Array = new ArraySegment<byte>(Data);
                await Socket.SendAsync(Array, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }
}