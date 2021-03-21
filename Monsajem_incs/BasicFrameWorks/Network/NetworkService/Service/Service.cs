using System;
using Monsajem_Incs.Net.Base.Socket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monsajem_Incs.Serialization;
using System.Reflection;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static System.Runtime.Serialization.FormatterServices;

namespace Monsajem_Incs.Net.Base.Service
{
    internal class Service<AddressType>
        where AddressType:IComparable<AddressType>
    {
        private AddressType[] ServicesAddress = new AddressType[0];
        private Func<Task>[] Services = new Func<Task>[0];
        private IAsyncOprations Link;

        public Service(IAsyncOprations link)
        {
            this.Link = link;
        }

        public void AddService(
            AddressType ServiceAddress,
            Func<Task> Service)
        {
            var Pos = BinaryInsert(ref ServicesAddress, ServiceAddress);
            Insert(ref Services, Service, Pos);
        }

        public async Task Response()
        {
            while (await Link.GetCondition())
            {
                var ServiceName = await Link.GetData<AddressType>();
                var Pos = System.Array.BinarySearch(ServicesAddress, ServiceName);
                await Services[Pos]();
            }
        }

        public async Task Request(AddressType ServiceAddress)
        {
            await Link.SendCondition(true);
            await Link.SendData(ServiceAddress);
        }

        public async Task CloseRequest()
        {
            await Link.SendCondition(false);
        }
    }
}