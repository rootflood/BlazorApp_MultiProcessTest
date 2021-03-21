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

        protected override StreamCollection<ValueType> MakeSameNew()
        {
            throw new NotImplementedException();
        }

        protected override void AfterAddLength(int Count)
        {
            Info.Keys.Insert(new Data[Count]);
        }

        protected override void OnDeleteFrom(int from)
        {
            throw new NotImplementedException();
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetFromTo(int From, int Len)
        {
            throw new NotImplementedException();
        }

        public override void SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            throw new NotImplementedException();
        }
    }
}
