using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monsajem_Incs.Serialization;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.StreamCollection
{

    public partial class StreamCollection<ValueType>
    {
        public override void Insert(ValueType Data, int Position)
        {
            InnerInser(Data.Serialize(), Position);
        }

        private void InnerInser(byte[] DataAsByte, int Pos)
        {
#if DEBUG
            Info.Browse(this);
#endif
            var DataLen = DataAsByte.Length;
            Data Gap=default;
            Data Data;
            if (Info.PopGapMinLen(DataLen,ref Gap)==false)//// haven't free gap
            {
                var StreamLen = (int)Info.StreamLen;
                AddLen(DataLen);
                Data = new Data()
                {
                    From = StreamLen,
                    Len = DataLen,
                    To = StreamLen + DataLen - 1
                };
            }
            else
            {
                Data = new Data()
                {
                    From = Gap.From,
                    Len = DataLen,
                    To = Gap.From + DataLen - 1
                };
                if (Gap.Len > DataLen)
                {
                    Gap.Len = Gap.Len - DataLen;
                    Gap.From = Gap.From + DataLen;
                    var NextGap = Gap;
                    if(Info.PopNextGap(ref NextGap))
                    {
                        Gap.Len += NextGap.Len;
                        Gap.To = NextGap.To;
                    }
                    Info.InsertGap(Gap);
                }
            }
            Info.InsertData(Data, Pos);
            
            Stream.Seek(Data.From, System.IO.SeekOrigin.Begin);
            Stream.Write(DataAsByte, 0, Data.Len);
            Stream.Flush();
            Length += 1;
#if DEBUG
            Info.Browse(this);
#endif
        }
    }
}
