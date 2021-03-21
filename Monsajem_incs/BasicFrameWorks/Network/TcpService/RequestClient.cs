using Monsajem_Incs.Net.Tcp.Socket;
using System;
using System.Threading;
using Monsajem_Incs.Net.Base.Service;

namespace Monsajem_Incs.Net.Tcp
{
    public class Client :
        Client<System.Net.EndPoint>
    {
        public Client() :
            base(new TcpClientSocket())
        { }
    }
}
