using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Monsajem_Incs.Net.Tcp.Socket
{
    public partial class TcpClientSocket : Base.Socket.ClientSocket<System.Net.EndPoint>
    {
        public System.Net.Sockets.Socket ClientSocket;

        internal new bool IsConnected { set => base.IsConnected = value; }

        public TcpClientSocket(System.Net.Sockets.Socket ClientSocket)
        {
            this.ClientSocket = ClientSocket;
        }
        public TcpClientSocket()
        { }

        protected override async Task Inner_Connect(EndPoint Address)
        {
            this.ClientSocket = new System.Net.Sockets.Socket(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);
            ClientSocket.Connect(Address);
        }

        protected override async Task Inner_Disconnect()
        {
            ClientSocket.Close();
        }

        protected override async Task Inner_Send(byte[] Data)
        {
            ClientSocket.Send(Data);
        }

        public override async Task<int> Recive(byte[] Buffer)
        {
            var Recived = ClientSocket.Receive(Buffer);
            if (Recived == 0)
            {
                ClientSocket.Close();
                ClientSocket.Receive(Buffer);
            }
            return Recived;
        }
    }
}
