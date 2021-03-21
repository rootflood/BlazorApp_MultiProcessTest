using System;
using System.Net;
using System.Threading.Tasks;

namespace Monsajem_Incs.Net.Tcp.Socket
{
    public class TcpServerSocket:Base.Socket.ServerSocket<System.Net.EndPoint>
    {
        public System.Net.Sockets.Socket ServerSocket;

        public TcpServerSocket(System.Net.Sockets.Socket ServerSocket) :
            base(
                BeginService:(address)=> { ServerSocket.Bind(address); ServerSocket.Listen(int.MaxValue); },
                Disconnect:()=> ServerSocket.Disconnect(true),
                WaitForAccept:()=> new TcpClientSocket(ServerSocket.Accept()) { IsConnected = true })
        {
            this.ServerSocket = ServerSocket;
        }

        public TcpServerSocket():
            this(new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp))
        { }
    }

    

}
