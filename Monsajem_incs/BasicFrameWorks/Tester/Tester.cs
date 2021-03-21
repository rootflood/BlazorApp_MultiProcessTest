using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monsajem_Incs.Serialization;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
namespace Monsajem_Incs.TimeingTester
{
    public static class Timing
    {
        public static TimeSpan run(Action Somthing)
        {
            Action InnerAction=()=>{};
            Stopwatch HaveLate = new Stopwatch();
            HaveLate.Start();
            InnerAction();
            HaveLate.Stop();


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Somthing();
            stopWatch.Stop();           

            return stopWatch.Elapsed - HaveLate.Elapsed;
        }

        public static async Task<TimeSpan> run(Func<Task> Somthing)
        {
            Func<Task> InnerAction = async () => { };
            Stopwatch HaveLate = new Stopwatch();
            HaveLate.Start();
            await InnerAction();
            HaveLate.Stop();


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            await Somthing();
            stopWatch.Stop();

            return stopWatch.Elapsed - HaveLate.Elapsed;
        }
    }

    public static class StreamLoger
    {
        public static System.IO.Stream Stream;
        private static int CurrentPos;
        private static int _StreamLen;
        private static int StreamLen
        {
            get => _StreamLen;
            set
            {
                _StreamLen = value;
                if (_StreamLen < MinLen)
                {
                    MaxLen = _StreamLen + minCount;
                    MinLen = _StreamLen - minCount;
                    Stream.SetLength(MaxLen);
                }
            }
        }
        private static int MaxLen=1000;
        private static int MinLen=-1000;
        private static int minCount=1000;
        private static int ActionsCount;
        public static void run(Action Action)
        {
            lock (Stream)
            {
                bool IsDone = false;
                var ActionByte = Action.Serialize();
                Action Save = () =>
                {
                    if(ActionsCount==0)
                        StreamLen += 4;
                    ActionsCount += 1;
                    Stream.Seek(0, System.IO.SeekOrigin.Begin);
                    Stream.Write(BitConverter.GetBytes(ActionsCount), 0, 4);

                    Stream.Seek(StreamLen, System.IO.SeekOrigin.Begin);
                    StreamLen += ActionByte.Length + 5;
                    if (IsDone)
                        Stream.Write(new byte[] { 1 }, 0, 1);
                    else
                        Stream.Write(new byte[] { 0 }, 0, 1);
                    Stream.Write(BitConverter.GetBytes(ActionByte.Length), 0, 4);
                    Stream.Write(ActionByte, 0, ActionByte.Length);
                    Stream.Flush();
                };
                try
                {
                    Action();
                    IsDone = true;
                    Save();
                }
                catch
                {
                    IsDone = false;
                    Save();
                    throw;
                }
            }
        }

        private static byte[] Read(int Count)
        {
            var data = new byte[Count];
            var Pos = 0;
            while(Count>0)
            {
               var C= Stream.Read(data, Pos, Count);
                if (C == 0)
                    throw new Exception("Stream Error");
                Count -= C;
                Pos += C;
            }
            return data;
        }
        public static void DebugStream(Action<(Action Action, bool IsSafe)> Action)
        {
            Stream.Seek(0, System.IO.SeekOrigin.Begin);
            var data = Read(4);
            ActionsCount = BitConverter.ToInt32(data, 0);
            for(int i=0;i<ActionsCount;i++)
            {
                data = Read(1);
                bool IsSafe = data[0] == 1;
                data = Read(4);
                var Len = BitConverter.ToInt32(data, 0);
                data = Read(Len);
                var Ac = data.Deserialize<Action>();
                Action((Ac, IsSafe));
            }
        }
    }
}