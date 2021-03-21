using System;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static System.Text.Encoding;

namespace Monsajem_Incs.Serialization
{
    public partial class Serialization
    {
        [ThreadStatic]
        private static Action AtLast;
        private void VisitedSerialize(
            object obj,
            SerializeInfo serializer)
        {
            if (obj == null)
            {
                S_Data.Write(Byte_Int_N_2, 0, 4);
                return;
            }
            if (serializer.CanStoreInVisit == false)
            {
                S_Data.Write(Byte_Int_N_1, 0, 4);
                serializer.Serializer(obj);
                return;
            }
            var VisitedObj = new ObjectContainer()
            {
                ObjHashCode = obj.GetHashCode(),
                TypeHashCode = serializer.TypeHashCode
            };
            var VisitedPos = Visitor.BinaryInsert(ref VisitedObj);
            if (VisitedPos > -1)
            {
#if DEBUG
                if (VisitedObj.obj.GetType() != obj.GetType())
                    throw new Exception("Type of visited object is wrong" +
                                        "\nMain: " + obj.GetType().ToString() +
                                        "\nVisited: " + VisitedObj.obj.GetType().ToString());
#endif
                S_Data.Write(BitConverter.GetBytes(VisitedObj.FromPos), 0, 4);
                return;
            }
#if DEBUG
            VisitedObj.obj = obj;
#endif
            VisitedObj.FromPos = (int)S_Data.Position;
            S_Data.Write(Byte_Int_N_1, 0, 4);
            serializer.Serializer(obj);
        }
        private void VisitedDeserialize(
            Action<object> Set,
            SerializeInfo deserializer)
        {
            var LastFrom = From;
            var Fr = BitConverter.ToInt32(D_Data, From);
            From += 4;
            if (deserializer.CanStoreInVisit == false)
            {
                if (Fr == -2)
                    return;
                Set(deserializer.Deserializer());
                return;
            }
            ObjectContainer VisitedObj;
            switch (Fr)
            {
                case -1:
                    VisitedObj = new ObjectContainer()
                    {
                        ObjHashCode = LastFrom
                    };
                    Visitor.BinaryInsert(VisitedObj);
                    VisitedObj.obj = deserializer.Deserializer();
                    Set(VisitedObj.obj);
                    return;
                case -2:
                    return;
            }
            VisitedObj = new ObjectContainer()
            {
                ObjHashCode = Fr
            };
            VisitedObj = Visitor.BinarySearch(VisitedObj).Value;
            if (VisitedObj.obj == null)
                AtLast += () => Set(VisitedObj.obj);
            else
                Set(VisitedObj.obj);
        }

        private t VisitedInfoSerialize<t>(
            int HashCode,
            Func<(byte[], t)> GetData)
        {
            var VisitedObj = new ObjectContainer()
            {
                ObjHashCode = HashCode
            };
            int VisitedPos = Visitor_info.BinaryInsert(ref VisitedObj);
            if (VisitedPos > -1)
            {
                S_Data.Write(BitConverter.GetBytes(VisitedObj.FromPos), 0, 4);
                return (t)VisitedObj.obj;
            }
            VisitedObj.FromPos = (int)S_Data.Position;
            var Data = GetData();
            VisitedObj.obj = Data.Item2;
            S_Data.Write(Byte_Int_N_1, 0, 4);
            S_Data.Write(Data.Item1, 0, Data.Item1.Length);
            return Data.Item2;
        }
        private t VisitedInfoDeserialize<t>(
            Func<t> Get)
        {
            var LastFrom = From;
            var Fr = BitConverter.ToInt32(D_Data, From);
            From += 4;
            ObjectContainer VisitedObj;
            if (Fr == -1)
            {
                VisitedObj = new ObjectContainer()
                {
                    ObjHashCode = LastFrom,
                    obj = Get()
                };
                Visitor_info.BinaryInsert(VisitedObj);
                return (t)VisitedObj.obj;
            }
            VisitedObj = new ObjectContainer()
            {
                ObjHashCode = Fr
            };
            return (t)Visitor_info.BinarySearch(VisitedObj).Value.obj;
        }



        private byte[] Write(params string[] str)
        {
            byte[] Results = new byte[0];
            for (int i = 0; i < str.Length; i++)
            {
                var UTF8_Data = UTF8.GetBytes(str[i]);
                var Result = new byte[UTF8_Data.Length + 4];
                System.Array.Copy(BitConverter.GetBytes(UTF8_Data.Length), 0, Result, 0, 4);
                System.Array.Copy(UTF8_Data, 0, Result, 4, UTF8_Data.Length);
                Insert(ref Results, Result);
            }
            return Results;
        }

        private string Read()
        {
            var Len = BitConverter.ToInt32(D_Data, From);
            From += 4;
            var Result = UTF8.GetString(D_Data, From, Len);
            From += Result.Length;
            return Result;
        }
    }

}
