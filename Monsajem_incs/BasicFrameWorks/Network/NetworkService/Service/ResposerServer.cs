using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Monsajem_Incs.Net.Base.Socket;

namespace Monsajem_Incs.Net.Base.Service
{
    public class Server<AddressType>
    {
        private ServerSocket<AddressType> ServerSocket;

        public Server(
            ServerSocket<AddressType> ServerSocket)
        {
            this.ServerSocket = ServerSocket;
        }

        public void StartServicing(
            AddressType Address,
            Action<SyncOprations<AddressType>> Service)
        {
           new Thread(() =>
           {
               ServerSocket.BeginService(Address);
               while (true)
               {
                   var Client = ServerSocket.WaitForAccept();
                   new Thread(() =>
                   {
                       Service(new SyncOprations<AddressType>(Client, true));
#if DEBUG
                       Client.WhyDisconnect = "end";
#endif
                       Client.Disconncet().Wait();
                   }).Start();
               }
           }).Start();
        }
    }
}