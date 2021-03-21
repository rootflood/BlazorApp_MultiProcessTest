using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monsajem_Incs.Serialization;

namespace Monsajem_Incs.StreamCollection
{
    public partial class StreamCollection<ValueType>
    {
        public ValueType GetItem(int Position)
        {
            //Info.Browse();
            var DataLoc = Info.GetData(Position);
            var DataAsByte = new byte[DataLoc.Len];
            Stream.Seek(DataLoc.From, System.IO.SeekOrigin.Begin);
            var Len = DataLoc.Len;
            var Pos = 0;
            while(Len>0)
            {
                var CLen = Stream.Read(DataAsByte,Pos, Len);
                if (CLen == 0)
                    throw new Exception("Stream Error");
                Len -= CLen;
                Pos += CLen;
            }
            return DataAsByte.Deserialize<ValueType>();
        }

        public void SetItem(int Pos,ValueType value)
        {
#if DEBUG
            Info.Browse(this);
            value.Serialize().Deserialize(value);
#endif
            var Data = value.Serialize();
            var DataLoc = Info.GetData(Pos);
            if(DataLoc.Len!=Data.Length)
            {
                var Gap = DataLoc;
                if (Info.PopNextGap(ref Gap))
                {
                    DataLoc.To = Gap.To;
                    DataLoc.Len += Gap.Len;
                }
                if (DataLoc.To == Info.StreamLen - 1)//// is last data;
                {
                    var SpaceLen = DataLoc.Len;
                    DataLoc.Len = Data.Length;
                    if (DataLoc.Len < SpaceLen)
                    {
                        SpaceLen = SpaceLen - DataLoc.Len;
                        DeleteLen(SpaceLen);
                        DataLoc.To -= SpaceLen;
                    }
                    else if (DataLoc.Len > SpaceLen)
                    {
                        SpaceLen = DataLoc.Len - SpaceLen;
                        AddLen(SpaceLen);
                        DataLoc.To += SpaceLen;
                    }
                    Info.Keys[Pos] = DataLoc;
                }
                else
                {
                    Gap = DataLoc;
                    if (Info.PopBeforeGap(ref Gap))
                    {
                        DataLoc.From = Gap.From;
                        DataLoc.Len += Gap.Len;
                    }
                    if (DataLoc.Len < Data.Length)
                    {
                        Info.DeleteData(Pos);
                        Length -= 1;
                        Info.InsertGap(DataLoc);
                        InnerInser(Data, Pos);
                    }
                    else
                    {
                        DataLoc.To = DataLoc.From + Data.Length - 1;
                        if (DataLoc.Len > Data.Length)
                        {
                            Gap = new Data()
                            {
                                From = DataLoc.To + 1,
                                Len = DataLoc.Len - Data.Length,
                            };
                            Gap.To = Gap.From + Gap.Len - 1;
                            var NextGap = Gap;
                            if (Info.PopNextGap(ref NextGap))
                            {
                                Gap.Len += NextGap.Len;
                                Gap.To = NextGap.To;
                            }
                            Info.InsertGap(Gap);
                        }
                        DataLoc.Len = Data.Length;
                        Info.Keys[Pos] = DataLoc;
                    }
                }
            }
            Stream.Seek(DataLoc.From, System.IO.SeekOrigin.Begin);
            Stream.Write(Data, 0, DataLoc.Len);
            Stream.Flush();
#if DEBUG
            Info.Browse(this);
#endif
        }

        public override ValueType this[int Pos] { 
            get => GetItem(Pos);
            set => SetItem(Pos, value);
        }
    }
}
