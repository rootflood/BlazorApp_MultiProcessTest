using System;
using System.Threading.Tasks;

namespace Monsajem_Incs.Net.Base.Socket
{

    public class ServerSocket<Address>
    {
        private Action<Address> P_BeginService;
        private Action P_Disconnect;
        private Func<ClientSocket<Address>> P_WaitForAccept;

        public ServerSocket(
            Action<Address> BeginService=null,
            Action Disconnect=null,
            Func<ClientSocket<Address>> WaitForAccept=null)
        {
            P_BeginService = BeginService;
            P_Disconnect = Disconnect;
            P_WaitForAccept = WaitForAccept;
        }


        public void BeginService(Address address) => OnBeginService(address);
        public void Disconnect() => OnDisconnect();
        public ClientSocket<Address> WaitForAccept() => OnWaitForAccep();

        protected virtual void OnBeginService(Address Address)=> P_BeginService(Address);
        protected virtual void OnDisconnect()=> P_Disconnect();
        protected virtual ClientSocket<Address> OnWaitForAccep()=> P_WaitForAccept();
    }
}
