using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.Serialization;
using Monsajem_Incs.Database;
using Monsajem_Incs.Array.Hyper;

namespace Monsajem_Incs.StreamCollection
{
    internal struct Data:IComparable<Data>
    {
        public int From;
        public int To;
        public int Len;

        public int CompareTo(Data other)
        {
            return From - other.From;
        }
    }
    internal struct DataByForm : IComparable<DataByForm>
    {
        public Data Data;
        public int CompareTo(DataByForm other)
        {
            return Data.From - other.Data.From;
        }
    }
    internal struct DataByTo : IComparable<DataByTo>
    {
        public Data Data;
        public int CompareTo(DataByTo other)
        {
            return Data.To - other.Data.To;
        }
    }
    internal struct DataByLen : IComparable<DataByLen>
    {
        public Data Data;
        public int CompareTo(DataByLen other)
        {
            var c = Data.Len - other.Data.Len;
            if (c == 0)
                c = Data.From - other.Data.From;
            return c;
        }
    }

    public partial class StreamCollection<ValueType> :
        Array.Base.IArray<ValueType, StreamCollection<ValueType>>,
        ISerializable<StreamCollection<ValueType>.MyData>
    {
#if DEBUG
        public MyData Info = new MyData();
#else
        private MyData Info = new MyData();
#endif

#if DEBUG
        public
#else
        internal
#endif
            class MyData:ISerializable<(Array<Data> Keys, long StreamLen)>
        {
            internal Array<Data> Keys;
            internal SortedArray<DataByForm> GapsByFrom;
            internal SortedArray<DataByLen> GapsByLen;
            internal SortedArray<DataByTo> GapsByTo;

            internal long MinLen = -500;
            internal long MaxLen = 500;
            internal long minCount = 500;
            [CopyOrginalObject]
            public long StreamLen;

            internal Data GetData(int Pos)
            {
                return Keys[Pos];
            }
            internal Data PopData(int Pos)
            {
                var Result = Keys[Pos];
                Keys.DeleteByPosition(Pos);
                return Result;
            }
            internal void InsertData(Data Data, int Pos)
            {
                Keys.Insert(Data, Pos);
            }
            internal void DeleteData(int Pos)
            {
                Keys.DeleteByPosition(Pos);
            }
            internal bool PopGapMinLen(int Len, ref Data Gap)
            {
                var Finded =
                    GapsByLen.BinarySearch(new DataByLen() { Data = new Data() { Len = Len } });
                var Index = Finded.Index;
                if (Index < 0)
                {
                    Index = Index * -1;
                    if (Index > GapsByLen.Length)
                        return false;
                    else
                    {
                        Gap = GapsByLen[Index - 1].Data;
                        DeleteGap(Gap);
                        return true;
                    }
                }
                else
                {
                    Gap = Finded.Value.Data;
                    DeleteGap(Gap);
                    return true;
                }
            }

            internal bool PopNextGap(ref Data Data)
            {
                var Finded =
                    GapsByFrom.BinarySearch(new DataByForm()
                    {
                        Data = new Data() { From = Data.To + 1 }
                    });
                if (Finded.Index > -1)
                {
                    Data = Finded.Value.Data;
                    DeleteGap(Data);
                    return true;
                }
                return false;
            }
            internal bool PopBeforeGap(ref Data Data)
            {
                var Finded =
                    GapsByTo.BinarySearch(new DataByTo()
                    {
                        Data = new Data() { To = Data.From - 1 }
                    });
                if (Finded.Index > -1)
                {
                    Data = Finded.Value.Data;
                    DeleteGap(Data);
                    return true;
                }
                return false;
            }

            internal void InsertGap(Data Gap)
            {
                GapsByFrom.BinaryInsert(new DataByForm() { Data = Gap });
                GapsByLen.BinaryInsert(new DataByLen() { Data = Gap });
                GapsByTo.BinaryInsert(new DataByTo() { Data = Gap });
            }
            internal void DeleteGap(Data Gap)
            {
#if DEBUG
                if (
#endif
                    GapsByFrom.BinaryDelete(new DataByForm() { Data = Gap })
#if DEBUG
                   .Index < 0)throw new Exception()
#endif
                    ;
#if DEBUG
                if (
#endif
                GapsByLen.BinaryDelete(new DataByLen() { Data = Gap })
#if DEBUG
                  .Index < 0) throw new Exception()
#endif
                        ;
#if DEBUG
                if (
#endif
                    GapsByTo.BinaryDelete(new DataByTo() { Data = Gap })
#if DEBUG
                  .Index < 0) throw new Exception()
#endif
                        ;
            }
#if DEBUG
            public void Browse(StreamCollection<ValueType> Parent)
            {
                if (Parent.Length != this.Keys.Length)
                    throw new Exception("miss match Len");
                if (GapsByFrom.Where((c) =>
                     GapsByLen.BinarySearch(new DataByLen() { Data = c.Data }).Index < 0).
                    Count()>0)
                    throw new Exception();
                var Datas = new List<(string Role, Data Data)>(
                    GapsByFrom.Select((c) => ("isGap", c.Data)));
                foreach (var Key in this.Keys)
                    Datas.Add(("key", Key));
                var Keys = Datas.OrderBy((c) => c.Data.From).ToArray();
                for (int i = 0; i < Keys.Length; i++)
                {
                    Browse(i, Keys);
                }
            }
            internal void Browse(
                int i,
                (string Role, Data Data)[] Keys)
            {
                var Key = Keys[i];
                if (Key.Data.To >= StreamLen)
                    throw new Exception();
                if (Key.Data.Len != (Key.Data.To - Key.Data.From) + 1)
                    throw new Exception();
                if (Keys.Length > i + 1)
                {
                    var NextKey = Keys[i + 1];
                    if (Key.Data.To + 1 != NextKey.Data.From)
                        throw new Exception();
                    if(Key.Role== "isGap"& NextKey.Role== "isGap")
                        throw new Exception();
                }
                else
                {
                    if (Key.Data.To != StreamLen - 1)
                        throw new Exception();
                }
            }
#endif
            (Array<Data> Keys, long StreamLen) ISerializable<(Array<Data> Keys, long StreamLen)>.
                GetData()
            {
                return (Keys, StreamLen);
            }

            void ISerializable<(Array<Data> Keys, long StreamLen)>.
                SetData((Array<Data> Keys, long StreamLen) Data)
            {
                Keys = Data.Keys;
                StreamLen = Data.StreamLen;
                GapsByFrom = new SortedArray<DataByForm>();
                GapsByLen = new SortedArray<DataByLen>();
                GapsByTo = new SortedArray<DataByTo>();
                var NewDatas = new SortedArray<Data>();
                NewDatas.BinaryInsert(Keys.ToArray());
                var CurrentPos = 0;
                for(int i=0;i<NewDatas.Length;i++)
                {
                    var NewData = NewDatas[i];
                    if (CurrentPos != NewData.From)
                    {
                        InsertGap(new Data()
                        {
                            From = CurrentPos,
                            Len=(NewData.From-CurrentPos),
                            To = NewData.From-1
                        });
                    }
                    CurrentPos = NewData.To+1;
                }
            }
        }
    }
}