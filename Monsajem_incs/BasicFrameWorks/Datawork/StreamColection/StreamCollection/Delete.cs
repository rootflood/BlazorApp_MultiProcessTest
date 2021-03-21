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
        public override void DeleteByPosition(int Position)
        {
#if DEBUG
            Info.Browse(this);
#endif
            var DataLoc = Info.PopData(Position);
            var NGap = DataLoc;
            Data BGap;
            if (Info.PopNextGap(ref NGap))
            {
                DataLoc.To = NGap.To;
                DataLoc.Len += NGap.Len;
            }
            if (DataLoc.To == Info.StreamLen - 1)//// is last data;
            {
                BGap = DataLoc;
                if (Info.PopBeforeGap(ref BGap))
                    DataLoc.Len += BGap.Len;
                DeleteLen(DataLoc.Len);
                Stream.Flush();
            }
            else
            {
                BGap = DataLoc;
                if (Info.PopBeforeGap(ref BGap))
                {
                    DataLoc.From = BGap.From;
                    DataLoc.Len += BGap.Len;
                }
                Info.InsertGap(DataLoc);
            }
            Length -= 1;
#if DEBUG
            Info.Browse(this);
#endif
        }

        protected override StreamCollection<ValueType> MakeSameNew()
        {
            throw new NotImplementedException();
        }

        protected override void AddLength(int Count)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFrom(int from)
        {
            throw new NotImplementedException();
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetFromTo(int From, int Len)
        {
            throw new NotImplementedException();
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetAllArrays()
        {
            throw new NotImplementedException();
        }

        public override void SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            throw new NotImplementedException();
        }

        protected override void AddFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            throw new NotImplementedException();
        }
    }
}
