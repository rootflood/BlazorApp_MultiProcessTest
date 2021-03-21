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

    public class RemoteExactDelegate:Attribute
    {}
    public interface IAsyncOprations
    {

        Task<bool> SendCondition(bool Condition);
        Task<bool> GetCondition();
        Task SendData<t>(t Data);
        Task<t> GetData<t>();
        Task<t> GetData<t>(t SampleType);
        Task SendArray<t>(IEnumerable<t> Datas, Action<t> DataSended = null);
        Task GetArray<t>(Action<t> DataCome);
        Task<t[]> GetArray<t>();
        Task Remote<t>(t obj);
        Task RunOnOtherSide(Func<IAsyncOprations, Task> Action);
        Task RunOnOtherSide<Data>(Func<IAsyncOprations, Data,Task> Action);
        Task RunOnOtherSide(Func<Task> Action);
        Task RunOnOtherSide<Data>(Func<Data,Task> Action);
        Task<Result> RunOnOtherSide<Result>(Func<Task<Result>> Func);
        Task<Result> RunOnOtherSide<Result, Data>(Func<Data,Task<Result>> Func);
        Task RunOnOtherSide(Action<ISyncOprations> Action);
        Task RunOnOtherSide<Data>(Action<ISyncOprations, Data> Action);
        Task RunOnOtherSide(Action Action);
        Task RunOnOtherSide<Data>(Action<Data> Action);
        Task<Result> RunOnOtherSide<Result>(Func<Result> Func);
        Task<Result> RunOnOtherSide<Result, Data>(Func<Data, Result> Func);
        Task RunRecivedAction<DataType>(Action<Delegate> Permition, DataType Data);
        Task Sync(Action Action);
        Task Sync();
        Task Stop();
    }
    public class AsyncOprations<AddressType>:
        IDisposable,IAsyncOprations
    {
        internal ClientSocket<AddressType> Client;
        public event Action<AddressType> Misstake;
#if DEBUG
        private Func<OprationType, OprationType,Task> Parity;
        private int Sequnce = 0;
#endif
        private bool IsServer;
        public AsyncOprations(
            ClientSocket<AddressType> Client,bool IsServer)
        {
            this.Client = Client;
            this.IsServer = IsServer;
#if DEBUG
            if (IsServer)
                Parity = ServerParity;
            else
                Parity = ClientParity;
#endif
        }

        private t Deserialize<t>(byte[] arrBytes)
        {
            if(IsServer)
            {
                bool IsSafe = false;
                try
                {
                    var Result = arrBytes.Deserialize<t>();
                    IsSafe = true;
                    return Result;
                }
                finally
                {
                    if (IsSafe == false)
                    {
                        Misstake?.Invoke(Client.Address);
                    }
                }
            }
            else
            {
                return arrBytes.Deserialize<t>();
            }
        }

        public async Task<bool> SendCondition(bool Condition)
        {
#if DEBUG
            await Parity(OprationType.SendCondition, OprationType.GetCondition);
#endif
            if (Condition == true)
                await Client.Send(new byte[] { 0 });
            else
                await Client.Send(new byte[] { 1 });
            return Condition;
        }

        public async Task<bool> GetCondition()
        {
#if DEBUG
            await Parity(OprationType.GetCondition, OprationType.SendCondition);
#endif
            return (await Client.Recive(1))[0] == 0;
        }

        public async Task SendData<t>(t Data)
        {
#if DEBUG
            await Parity(OprationType.SendData, OprationType.GetData);
#endif
            await Client.SendPacket(Data.Serialize());
        }

        public async Task<t> GetData<t>()
        {
#if DEBUG
            await Parity(OprationType.GetData, OprationType.SendData);
#endif
            t Data = Deserialize<t>(await Client.RecivePacket());
            return Data;
        }

        public async Task<t> GetData<t>(t SampleType)
        {
            return await GetData<t>();
        }

        public async Task SendArray<t>(IEnumerable<t> Datas, Action<t> DataSended = null)
        {
            await Client.Send(BitConverter.GetBytes(Datas.Count()));
            foreach (var data in Datas)
            {
                await Client.SendPacket(data.Serialize());
                DataSended?.Invoke(data);
            }
        }

        public async Task GetArray<t>(Action<t> DataCome)
        {
            for (int i = BitConverter.ToInt32(await Client.Recive(4),0); i > 0; i--)
                DataCome((await Client.RecivePacket()).Deserialize<t>());
        }

        public async Task<t[]> GetArray<t>()
        {
            var Datas = new t[BitConverter.ToInt32(await Client.Recive(4), 0)];
            for (int i = 0; i < Datas.Length; i++)
                Datas[i] = (await Client.RecivePacket()).Deserialize<t>();
            return Datas;
        }

        internal static (FieldInfo[] Remotes, FieldInfo[] Exacts) GetDeletageFields(
            Type typeToReflect, 
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            var Remotes = typeToReflect.GetFields(bindingFlags);
            if (filter != null)
                Remotes = Remotes.Where((c) => filter(c)& c.FieldType.BaseType == typeof(MulticastDelegate)).ToArray();
            else
                Remotes = Remotes.Where((c) =>c.FieldType.BaseType == typeof(MulticastDelegate)).ToArray();
            var Exacts = Remotes.Where((c) => c.GetCustomAttributes(typeof(RemoteExactDelegate)).Count() > 0).ToArray();
            Remotes = Remotes.Where((c) => c.GetCustomAttributes(typeof(RemoteExactDelegate)).Count() == 0).ToArray();
            if (typeToReflect.BaseType != null)
            {
                var BaseFields = GetDeletageFields(typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
                Insert(ref Remotes, BaseFields.Remotes);
                Insert(ref Exacts, BaseFields.Exacts);
            }
            return (Remotes,Exacts);
        }

        public async Task Remote<t>(t obj)
        {
            var Fields = GetDeletageFields(typeof(t));
            var Service = new Service<string>(this);
            foreach (var Field in Fields.Remotes)
            {
                if (Field.FieldType.GetMethod("Invoke").ReturnType != typeof(void))
                {
                    Service.AddService(Field.Name, async () =>
                    {
                        var Params = await GetData<object[]>();
                        object Rs = null;
                        await Sync(() => { Rs = ((Delegate)Field.GetValue(obj)).DynamicInvoke(Params); });
                        await SendData(Rs);
                    });
                }
                else
                {
                    Service.AddService(Field.Name, async () =>
                    {
                        var Params = await GetData<object[]>();
                        await Sync(() => ((Delegate)Field.GetValue(obj)).DynamicInvoke(Params));
                    });
                }
            }
            await Service.Response();
        }

        //0
        public async Task RunOnOtherSide(Action<ISyncOprations> Action)
        {
            await SendData<byte>(0);
            await SendData(Action);
        }
        //1
        public async Task RunOnOtherSide<Data>(Action<ISyncOprations, Data> Action)
        {
            await SendData<byte>(1);
            await SendData(Action);
        }
        //2
        public async Task RunOnOtherSide(Action Action)
        {
            await SendData<byte>(2);
            await SendData(Action);
        }
        //3
        public async Task RunOnOtherSide<Data>(Action<Data> Action)
        {
            await SendData<byte>(3);
            await SendData(Action);
        }
        //4
        public async Task<Result> RunOnOtherSide<Result>(Func<Result> Func)
        {
            await SendData<byte>(4);
            await SendData<Delegate>(Func);
            return (Result) await GetData<object>();
        }
        //5
        public async Task<Result> RunOnOtherSide<Result, Data>(Func<Data,Result> Func)
        {
            await SendData<byte>(5);
            await SendData<Delegate>(Func);
            return (Result)await GetData<object>();
        }
        //6
        public async Task RunOnOtherSide(Func<IAsyncOprations,Task> Action)
        {
            await SendData<byte>(6);
            await SendData<object>(Action);
        }
        //7
        public async Task RunOnOtherSide<Data>(Func<IAsyncOprations, Data,Task> Action)
        {
            await SendData<byte>(7);
            await SendData(Action);
        }
        //8
        public async Task RunOnOtherSide(Func<Task> Action)
        {
            await SendData<byte>(8);
            await SendData<object>(Action);
        }
        //9
        public async Task RunOnOtherSide<Data>(Func<Data,Task> Action)
        {
            await SendData<byte>(9);
            await SendData(Action);
        }
        //10
        public async Task<Result> RunOnOtherSide<Result>(Func<Task<Result>> Func)
        {
            await SendData<byte>(10);
            await SendData<Delegate>(Func);
            return await GetData<Result>();
        }
        //11
        public async Task<Result> RunOnOtherSide<Result, Data>(Func<Data, Task<Result>> Func)
        {
            await SendData<byte>(11);
            await SendData<Delegate>(Func);
            return await GetData<Result>();
        }
        public Task RunRecivedAction(
            Action<Delegate> Permition = null)
        {
            return RunRecivedAction<string>(Permition,null);
        }
        public async Task RunRecivedAction<DataType>(
            Action<Delegate> Permition,DataType Data)
        {
            var Type = await GetData<byte>();
            switch(Type)
            {
                case 0:
                    {
                        var dg = await GetData<Action<ISyncOprations>>();
                        Permition?.Invoke(dg);
                        dg.Invoke((ISyncOprations)this); break;
                    }
                case 1:
                    {
                        var dg = await GetData<Action<ISyncOprations, DataType>>();
                        Permition?.Invoke(dg);
                        dg.Invoke((ISyncOprations)this, Data); break;
                    }
                case 2:
                    {
                        var dg = await GetData<Action>();
                        Permition?.Invoke(dg);
                        dg.Invoke(); break;
                    }
                case 3:
                    {
                        var dg = await GetData<Action<DataType>>();
                        Permition?.Invoke(dg);
                        dg.Invoke(Data); break;
                    }
                case 4:
                    {
                        var dg = await GetData<Delegate>();
                        Permition?.Invoke(dg);
                        await SendData(dg.DynamicInvoke()); break;
                    }
                case 5:
                    {
                        var dg = await GetData<Delegate>();
                        Permition?.Invoke(dg);
                        await SendData(dg.DynamicInvoke(Data)); break;
                    }
                case 6:
                    {
                        var dg = await GetData<Func<IAsyncOprations,Task>>();
                        Permition?.Invoke(dg);
                        await dg.Invoke(this); break;
                    }
                case 7:
                    {
                        var dg = await GetData<Func<IAsyncOprations, DataType,Task>>();
                        Permition?.Invoke(dg);
                        await dg.Invoke((ISyncOprations)this, Data); break;
                    }
                case 8:
                    {
                        var dg = await GetData<Func<Task>>();
                        Permition?.Invoke(dg);
                        await dg.Invoke(); break;
                    }
                case 9:
                    {
                        var dg = await GetData<Func<DataType,Task>>();
                        Permition?.Invoke(dg);
                        await dg.Invoke(Data); break;
                    }
                case 10:
                    {
                        var dg = await GetData<Delegate>();
                        Permition?.Invoke(dg);
                        var Result = (Task)dg.DynamicInvoke();
                        await Result;
                        await SendData(Result.GetType().GetProperty("Result").GetValue(Result)); break;
                    }
                case 11:
                    {
                        var dg = await GetData<Delegate>();
                        Permition?.Invoke(dg);
                        var Result = (Task)dg.DynamicInvoke(Data);
                        await Result;
                        await SendData(Result.GetType().GetProperty("Result").GetValue(Result)); break;
                    }
                    throw new Exception();
            }
        }

        public async Task Sync(Action Action)
        {
            try
            {
                Action();
                await SendCondition(false);
            }
            catch(Exception ex)
            {
                await SendCondition(true);
#if DEBUG
                await SendData(ex.Message);
                await SendData(ex.StackTrace);
#else
                await SendData(ex.Message);
#endif
            }
        }

        public async Task Sync()
        {
            if(await GetCondition())
            {
#if DEBUG
                Exception ex = new Exception("Other side net sync Error >> " +
                    await GetData<string>()+"\n At >> " + await GetData<string>());
#else
                Exception ex =new Exception("Other side net sync Error >> "+
                    await GetData<string>());
#endif
                throw ex;
            }
        }

        public async Task Stop()
        {
#if DEBUG
            Client.WhyDisconnect = "End";
#endif
            await Client.Disconncet();
        }

        public void Dispose()
        {}

#if DEBUG

        private async Task ClientParity(OprationType ThisType, OprationType ThatType)
        {
#if TRACE_NET
            Console.WriteLine($"Net:{Client.Address} {ThisType.ToString()}   {ThatType.ToString()}");
            Console.WriteLine($"Net:{Client.Address} Sequence:{Sequnce}");
#endif

            await Client.Send(new byte[] { (byte)ThisType });
            await Client.Send(BitConverter.GetBytes(Sequnce));

            OprationType Status = (OprationType)(await Client.Recive(1))[0];
            int CurrentSequnce = BitConverter.ToInt32(await Client.Recive(4),0);

            if (Status == OprationType.Exeption)
                throw (await Client.RecivePacket()).Deserialize<Exception>();
            if (Sequnce != CurrentSequnce)
            {
                var Address = Client.Address.ToString();
                Client.WhyDisconnect = $"Wrong Sequnce On {Address}, This side is {Sequnce}" +
                                       $" but other side is {CurrentSequnce}";
                await Client.Disconncet();
                throw new InvalidOperationException(Client.WhyDisconnect);
            }

            Sequnce++;

            if (Status != ThatType)
            {
                var Address = Client.Address.ToString();
                Client.WhyDisconnect = "Wrong opration On " + Address + ", This side is '" + ThisType.ToString() + "'" +
                                      " And other side must be '" + ThatType.ToString() + "'" +
                                      " But that is '" + Status.ToString() + "'";
                await Client.Disconncet();
                throw new InvalidOperationException(Client.WhyDisconnect);
            }
        }

        private async Task ServerParity(
                      OprationType ThisType,
                      OprationType ThatType)
        {
#if TRACE_NET
            Console.WriteLine($"Net:{Client.Address} {ThisType.ToString()}   {ThatType.ToString()}");
            Console.WriteLine($"Net:{Client.Address} Sequence:{Sequnce}");
#endif
            OprationType Status = (OprationType)(await Client.Recive(1))[0];
            int CurrentSequnce = BitConverter.ToInt32(await Client.Recive(4), 0);

            await Client.Send(new byte[] { (byte)ThisType });
            await Client.Send(BitConverter.GetBytes(Sequnce));

            if (Sequnce != CurrentSequnce)
            {
                var Address = Client.Address.ToString();
                Client.WhyDisconnect = $"Wrong Sequnce On {Address}, This side is {Sequnce}" +
                                       $" but other side is {CurrentSequnce}";
                await Client.Disconncet();
                throw new InvalidOperationException(Client.WhyDisconnect);
            }

            Sequnce++;

            if (Status != ThatType)
            {
                var Address = Client.Address.ToString();
                Misstake?.Invoke(Client.Address);
                Client.WhyDisconnect =
                      "Wrong opration On " + Address + ", This side is '" + ThisType.ToString() + "'" +
                      " And other side must be '" + ThatType.ToString() + "'" +
                      " But that is '" + Status.ToString() + "'";
                await Client.Disconncet();
                throw new InvalidOperationException
                    (Client.WhyDisconnect);
            }
        }
#endif

        }
}