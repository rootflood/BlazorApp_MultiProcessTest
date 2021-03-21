using System;
using Monsajem_Incs.Net.Tcp.Socket;
using System.Threading;

namespace Monsajem_Incs.Net.Tcp
{
    public class Server:
        Base.Service.Server<System.Net.EndPoint>
    {
       private TcpServerSocket ServerSocket = new TcpServerSocket();
       public Server():
            base(new TcpServerSocket()){}
    }
}
