using Monsajem_Incs.Net.Base.Socket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monsajem_Incs.Net.Base.Service
{
    public class Client<AddressType> 
    {
        internal ClientSocket<AddressType> ClientSocket;
        public Client(
            ClientSocket<AddressType> ClientSocket)
        {
            this.ClientSocket = ClientSocket;
        }

        public async Task Connect(AddressType Address,
            Func<IAsyncOprations,Task> Requestor)
        {
            await ClientSocket.Connect(Address);
            using (var rq = new AsyncOprations<AddressType>(ClientSocket,false))
            {
                await Requestor(rq);
            }
#if DEBUG
            ClientSocket.WhyDisconnect = "end";
#endif
            await ClientSocket.Disconncet();
        }

        public void Connect(AddressType Address,
            Action<ISyncOprations> Requestor)
        {
            ClientSocket.Connect(Address).GetAwaiter().GetResult();
            using (var rq = new SyncOprations<AddressType>(ClientSocket, false))
            {
                Requestor(rq);
            }
#if DEBUG
            ClientSocket.WhyDisconnect = "end";
#endif
            ClientSocket.Disconncet().GetAwaiter().GetResult();
        }
    }
}