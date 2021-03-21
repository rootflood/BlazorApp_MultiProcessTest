using System;
using System.Threading.Tasks;
using System.Threading;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using Monsajem_Incs.Net.Base.Socket.Exceptions;
using Monsajem_Incs.DynamicAssembly;
using System.Linq;
using Monsajem_Incs.Threading;

namespace Monsajem_Incs.Net.Base.Socket
{
    namespace Exceptions
    {
        public class SocketExeption : Exception
        {
            public SocketExeption() { }
            public SocketExeption(string Message) : base(Message) { }
        }
        public class SocketClosedExeption : SocketExeption
        {
            public SocketClosedExeption() { }
            public SocketClosedExeption(string Message) : base(Message) { }
        }
        public class SocketClosingExeption : SocketExeption
        {
            public SocketClosingExeption() { }
            public SocketClosingExeption(string Message) : base(Message) { }
        }
        public class ConnectionFailedExeption : SocketExeption
        {
            public ConnectionFailedExeption() { }
            public ConnectionFailedExeption(string Message) : base(Message) { }
        }
    }

    public interface IClientSocket
    {
        Task Send(byte[] Data);
        Task<int> Recive(byte[] Buffer);

        Task<byte[]> Recive(int Size);
        Task SendPacket(byte[] Data);
        Task<byte[]> RecivePacket();
        Task Disconncet();
    }

    public class ClientSocket<AddressType> :
        IClientSocket
    {
#if DEBUG
        public string WhyDisconnect;
#endif

        private Locker<int> Sendings = new Locker<int>();
        public event Action<Exception> OnError;
        private AddressType P_Address;

#if DEBUG
        public static int SendTimeOut = 3000;
        public static int ReciveTimeOut = 3000;
#endif

        public ClientSocket()
        {
            IsConnected = false;
        }
        public virtual AddressType Address => P_Address;

        private Locker<bool> P_IsConnected = new Locker<bool>() { Value = true };
        public virtual bool IsConnected
        {
            get => P_IsConnected.LockedValue;
            internal set
            {
#if DEBUG
                if (WhyDisconnect == null)
                    WhyDisconnect = "Just Changed To "+value+" in " +
                        string.Concat(new System.Diagnostics.StackTrace(true).GetFrames().
                        Select((c) =>"\nMethod> "+c.GetMethod().DeclaringType.ToString()+"."+
                                                  c.GetMethod().Name+
                                     "\nFile> " + c.GetFileName()+
                                     "\nLine> " + c.GetFileLineNumber()));
#endif
                if (value != P_IsConnected.Value)
                {
                    P_IsConnected.Value = value;
                }
            }
        }

        private async Task _Send(byte[] Data)
        {
            try
            {
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Sending " + Data.Length);
#endif
                Task[] Tasks;
                using (P_IsConnected.Lock())
                {
                    if (P_IsConnected.Value == false)
                    {
                        var ExMSG = "socket connection Changed in _Send(byte[] Data)";
#if DEBUG
                        ExMSG += "\nWhyDisconnect :" + WhyDisconnect;
#endif
                        var Ex = new SocketClosedExeption(ExMSG);
                        OnError?.Invoke(Ex);
                        throw Ex;
                    }
                    Tasks = new Task[] { 
                        Inner_Send(Data), 
                        P_IsConnected.WaitForChange()};
                }
                using (Sendings.Lock())
                    Sendings.Value++;
#if DEBUG
                var ResultTask = await Task.WhenAny(Tasks).TimeOut(SendTimeOut);
#else
                var ResultTask = await Task.WhenAny(Tasks);
#endif
                if (P_IsConnected.LockedValue == false)
                {
                    var ExMSG = "socket connection Changed in _Send(byte[] Data)";
#if DEBUG
                    ExMSG += "\nWhyDisconnect :" + WhyDisconnect;
#endif
                    var Ex = new SocketClosedExeption(ExMSG);
                    OnError?.Invoke(Ex);
                    throw Ex;
                }
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Sended " + Data.Length);
#endif
            }
#if TRACE_NET
            catch (Exception ex)
            {
                Console.WriteLine($"Net:{Address} Send Faild " + Data.Length);
                Console.WriteLine(ex.Message);
                throw;
            }
#endif
            finally
            {
                using (Sendings.Lock())
                    Sendings.Value--;
            }
        }

        public async Task Send(byte[] Data)
        {
            await _Send(Data);
        }

        public virtual async Task<int> Recive(byte[] Buffer)
        {
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Reciveing ...");
#endif
            Task[] SomthingRecived = null;
            using (P_IsConnected.Lock() + RecivedBuffer.Lock())
            {
                if (RecivedBuffer.Value.Length == 0)
                {
                    if (P_IsConnected.Value == false)
                    {
                        var ExMSG = "socket connection Changed in Recive(byte[] Buffer)";
#if DEBUG
                        ExMSG += "\nWhyDisconnect :" + WhyDisconnect;
#endif
                        var Ex = new SocketClosedExeption(ExMSG);
                        OnError?.Invoke(Ex);
                        throw Ex;
                    }
                    SomthingRecived = new Task[] { RecivedBuffer.WaitForChange(),
                                                   P_IsConnected.WaitForChange()};
                }
            }
            if (SomthingRecived != null)
            {
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Reciveing Waiting ...");
#endif
#if DEBUG
                var ResultTask = await Task.WhenAny(SomthingRecived).TimeOut(ReciveTimeOut);
#else
                var ResultTask = await Task.WhenAny(SomthingRecived);
#endif
            }
            using (RecivedBuffer.Lock())
            {
                var R_Buffer = RecivedBuffer.Value;

#if TRACE_NET
                Console.WriteLine($"Net:{Address} Recived " + R_Buffer.Length);
#endif
                var Position = 0;
                if (Buffer.Length > R_Buffer.Length)
                    Position = R_Buffer.Length - 1;
                else
                    Position = Buffer.Length - 1;
                var Recived = PopTo(ref R_Buffer, Position);
                RecivedBuffer.Value = R_Buffer;
                Recived.CopyTo(Buffer, 0);
                return Recived.Length;
            }
        }

        public async Task Connect(AddressType Address)
        {
            if (P_IsConnected.Value == true)
            {
                var Ex = new ConnectionFailedExeption("Socket Is Connected");
                OnError?.Invoke(Ex);
                throw Ex;
            }
            P_Address = Address;
            await Inner_Connect(Address);
            P_IsConnected.Value = true;
        }

        public async Task Close()
        {
#if DEBUG
            if (WhyDisconnect == null)
                WhyDisconnect = "Close Method";
#endif
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Disconnecting Close ...");
#endif
#if DEBUG
            await Inner_Disconnect().TimeOut(ReciveTimeOut);
#else
            await Inner_Disconnect();
#endif
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Disconnected Close !");
#endif
            IsConnected = false;
            P_Address = default(AddressType);
        }

        public async Task Disconncet()
        {
#if DEBUG
            if (WhyDisconnect == null)
                WhyDisconnect = "Disconncet Method";
#endif
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Disconnceting ");
#endif
            CheckSends:
            Task WaitForSend = null;
            using (Sendings.Lock())
            {
                if (Sendings.Value > 0)
                {
#if TRACE_NET
                    Console.WriteLine($"Net:{Address} Disconnceting Waiting for Sending " + Sendings.Value);
#endif
                    WaitForSend = Sendings.WaitForChange();
                }
            }
            if (WaitForSend != null)
            {
                await WaitForSend;
                goto CheckSends;
            }

            try
            {
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Last hand shaking...");
#endif
                await Task_EX.CheckAll(Send(new byte[1]), Recive(1));
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Last hand shaked.");
#endif

#if TRACE_NET
                Console.WriteLine($"Net:{Address} Last hand shake 2/2");
#endif
            }
            catch
            {
#if TRACE_NET
                Console.WriteLine($"Net:{Address} Last hand shake faild but its ok.");
#endif
            }

            await Close();
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Disconnceted ");
#endif
        }

        public async Task<byte[]> Recive(int Size)
        {
#if TRACE_NET
            var OldSize = Size;
            Console.WriteLine($"Net:{Address} Reciving For Size 0/{OldSize}");
#endif
            var Result = new byte[Size];
            var ResultLen = Size;
            while (Size > 0)
            {
                var currentBuffer = new byte[Size];
                var currentBuffer_Size = await Recive(currentBuffer);
                System.Array.Copy(currentBuffer, 0, Result, ResultLen - Size,
                    currentBuffer_Size);
                Size -= currentBuffer_Size;
#if TRACE_NET

                Console.WriteLine($"Net:{Address} Reciving For Size {OldSize-Size}/{OldSize}");
#endif
            }
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Recived For Size " + OldSize);
#endif
            return Result;
        }
        public async Task SendPacket(byte[] Data)
        {
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Sending Packet " + Data.Length);
#endif
            await Send(BitConverter.GetBytes(Data.Length));
            await Send(Data);
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Sendeded Packet " + Data.Length);
#endif
        }
        public async Task<byte[]> RecivePacket()
        {
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Reciving Packet ");
#endif
            var PacketSize = BitConverter.ToInt32(await Recive(4), 0);
#if TRACE_NET
            Console.WriteLine($"Net:{Address} Reciving Packet " + PacketSize);
#endif
            var Result = await Recive(PacketSize);

#if TRACE_NET
            Console.WriteLine($"Net:{Address} Recived Packet " + PacketSize);
#endif
            return Result;
        }

        protected virtual Task Inner_Connect(AddressType Address) => throw new NotImplementedException("Inner_Connect Not Implemented");
        protected virtual Task Inner_Disconnect() => throw new NotImplementedException("Inner_Disconnect Not Implemented");
        protected virtual Task Inner_Send(byte[] Data) => throw new NotImplementedException("Inner_Send Not Implemented");

        private Locker<byte[]> RecivedBuffer = new Locker<byte[]>() { Value = new byte[0] };
        protected void Recived(byte[] Buffer)
        {
            using (RecivedBuffer.Lock())
            {
                var R_Buffer = RecivedBuffer.Value;
                Insert(ref R_Buffer, Buffer);
                RecivedBuffer.Value = R_Buffer;
            }
        }
    }
}