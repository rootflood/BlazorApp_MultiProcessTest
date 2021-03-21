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
    public interface ISyncOprations:
        IAsyncOprations
    {
        new bool SendCondition(bool Condition);
        new bool GetCondition();
        new void SendData<t>(t Data);
        new t GetData<t>();
        new t GetData<t>(t SampleType);
        new void SendArray<t>(IEnumerable<t> Datas, Action<t> DataSended = null);
        new void GetArray<t>(Action<t> DataCome);
        new t[] GetArray<t>();
        new void RunOnOtherSide(Func<IAsyncOprations, Task> Action);
        new void RunOnOtherSide<Data>(Func<IAsyncOprations, Data, Task> Action);
        new void RunOnOtherSide(Func<Task> Action);
        new void RunOnOtherSide<Data>(Func<Data, Task> Action);
        new Result RunOnOtherSide<Result>(Func<Task<Result>> Func);
        new Result RunOnOtherSide<Result, Data>(Func<Data, Task<Result>> Func);
        new void RunOnOtherSide(Action<ISyncOprations> Action);
        new void RunOnOtherSide<Data>(Action<ISyncOprations, Data> Action);
        new void RunOnOtherSide(Action Action);
        new void RunOnOtherSide<Data>(Action<Data> Action);
        new Result RunOnOtherSide<Result>(Func<Result> Func);
        new Result RunOnOtherSide<Result, Data>(Func<Data, Result> Func);
        new void RunRecivedAction<DataType>(Action<Delegate> Permition, DataType Data);
        new void Sync(Action Action);
        new void Sync();
        new void Stop();
    }
    public class SyncOprations<AddressType> :
        AsyncOprations<AddressType>,
        ISyncOprations
    {
        public SyncOprations(
            ClientSocket<AddressType> Client, bool IsServer):
            base(Client, IsServer){}

        public new bool SendCondition(bool Condition) =>
            base.SendCondition(Condition).GetAwaiter().GetResult();

        public new bool GetCondition() =>
            base.GetCondition().GetAwaiter().GetResult();

        public new void SendData<t>(t Data) =>
            base.SendData(Data).Wait();

        public new t GetData<t>() =>
            base.GetData<t>().GetAwaiter().GetResult();

        public new t GetData<t>(t SampleType) =>
            base.GetData(SampleType).GetAwaiter().GetResult();

        public new void SendArray<t>(IEnumerable<t> Datas, Action<t> DataSended = null) =>
            base.SendArray(Datas, DataSended).Wait();

        public new void GetArray<t>(Action<t> DataCome) =>
            base.GetArray(DataCome).Wait();

        public new t[] GetArray<t>() =>
            base.GetArray<t>().GetAwaiter().GetResult();

        public new void Sync(Action Action) =>
            base.Sync(Action).Wait();

        public new void Sync() =>
            base.Sync().Wait();

        public new void Stop() =>
            base.Stop().Wait();

        public void Remote<t>(t obj, Action<t> Talk)
        {
            var Fields = GetDeletageFields(typeof(t));

            Action<string> Rq = null;
            var Service = new Service<string>(this);
            foreach (var Field in Fields.Remotes)
            {
                Field.SetValue(obj,
                    DynamicAssembly.TypeController.CreateDelagate(Field,
                         (inputs) =>
                         {
                             lock(Service)
                             {
                                 Service.Request(Field.Name).GetAwaiter().GetResult();
                                 SendData(inputs);
                                 Sync();
                             }
                         },
                         (inputs) =>
                         {
                             lock (Service)
                             {
                                 Service.Request(Field.Name).GetAwaiter().GetResult();
                                 SendData(inputs);
                                 Sync();
                                 return GetData<object>();
                             }
                         }));
            }
            Talk(obj);
        }

        public new void RunRecivedAction<DataType>(Action<Delegate> Permition, DataType Data) =>
            base.RunRecivedAction(Permition,Data).GetAwaiter().GetResult();
        public void RunRecivedAction() =>
            base.RunRecivedAction().GetAwaiter().GetResult();

        public new void RunOnOtherSide(Func<IAsyncOprations, Task> Action)
        {
            throw new NotImplementedException();
        }

        public new void RunOnOtherSide<Data>(Func<IAsyncOprations, Data, Task> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new void RunOnOtherSide(Func<Task> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new void RunOnOtherSide<Data>(Func<Data, Task> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new Result RunOnOtherSide<Result>(Func<Task<Result>> Func) =>
            base.RunOnOtherSide(Func).GetAwaiter().GetResult();

        public new Result RunOnOtherSide<Result, Data>(Func<Data, Task<Result>> Func) =>
            base.RunOnOtherSide(Func).GetAwaiter().GetResult();

        public new void RunOnOtherSide(Action<ISyncOprations> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new void RunOnOtherSide<Data>(Action<ISyncOprations, Data> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new void RunOnOtherSide(Action Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new void RunOnOtherSide<Data>(Action<Data> Action) =>
            base.RunOnOtherSide(Action).GetAwaiter().GetResult();

        public new Result RunOnOtherSide<Result>(Func<Result> Func) =>
            base.RunOnOtherSide(Func).GetAwaiter().GetResult();

        public new Result RunOnOtherSide<Result, Data>(Func<Data, Result> Func) =>
            base.RunOnOtherSide(Func).GetAwaiter().GetResult();
    }
}