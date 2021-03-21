using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Base
{
    public class DynamicArray<ArrayType> :
       IArray<ArrayType>
    {
        public override object MyOptions { get => null; set { } }

        public Func<int, ArrayType> _GetItem;
        public Action<int, ArrayType> _SetItem;
        public Action<int> _DeleteFrom;
        public Action<int> _AddLength;
        public Action<((int From, int To, System.Array Ar)[] Ar, int MaxLen),int> _AddFromTo;
        public Func<int, int, ((int From, int To, System.Array Ar)[], int MaxLen)> _GetFromTo;
        public Action<((int From, int To, System.Array Ar)[] Ar, int MaxLen), int> _SetFromTo;
        public Action<int> _DeleteByPosition;

        public DynamicArray()
        {
            _GetFromTo =
            (From, To) =>
            {
                var Result = new ArrayType[To - From];
                var Len = Result.Length;
                for (int i = 0; i < Len; i++)
                    Result[i] = this[i + From];
                return (new (int, int, System.Array)[] { (0,Len,Result) },Len);
            };

            _SetFromTo = (Ar, From) =>
            {
                for (int ArPos = 0;ArPos<Ar.Ar.Length; ArPos++)
                {
                    var Current_Ar_Info = Ar.Ar[ArPos];
                    var Current_Ar =(ArrayType[]) Current_Ar_Info.Ar;
                    var I_From = Current_Ar_Info.From;
                    var I_To = Current_Ar_Info.To;
                    for (int i = I_From; i < I_To; i++)
                        this[From++] = Current_Ar[i];
                }
            };
        }

        public override ArrayType this[int Pos] { get => _GetItem(Pos); set => _SetItem(Pos, value); }

        public override void DeleteFrom(int from)
        {
            _DeleteFrom(from);
        }

        protected override void AddFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            _AddFromTo(Ar,From);
        }

        protected override void AddLength(int Count)
        {
            _AddLength(Count);
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetFromTo(int From, int To)
        {
            return _GetFromTo(From, To);
        }

        public override void SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            _SetFromTo(Ar, From);
        }

        public override void DeleteByPosition(int Position)
        {
            if (_DeleteByPosition != null)
                _DeleteByPosition(Position);
            else
                base.DeleteByPosition(Position);
        }
    }
}