using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Base
{
    public interface IArray
    {
        System.Array ToArray();
        object MyOptions { get; set; }
        Type ElementType { get; }
        void Insert(System.Array Array);
        void SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From);
        ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetFromTo(int From, int To);
        ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetAllArrays();
        void SetAllArrays(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar);
        void SetLen(int Len);
    }

    public abstract partial class IArray<ArrayType> :
        IArray,IEnumerable<ArrayType>
    {

        void IArray.SetLen(int Len) => Length = Len;
        public int Length;

        public abstract object MyOptions { get; set; }

        public Type ElementType => typeof(ArrayType);
        
        public abstract ((int From, int To, System.Array Ar)[] Ar, int MaxLen) GetFromTo(int From, int To);
        protected abstract void AddLength(int Count);

        public abstract ArrayType this[int Pos] { get; set; }

        public virtual void Clear()
        {
            DeleteFromTo(0, Length - 1);
        }

        protected void SetFromTo((int From, int To, ArrayType[] Ar) Ar, int MaxLen, int From)
        {
            SetFromTo((new (int, int, System.Array)[] { Ar }, MaxLen), From);
        }

        protected virtual void AddFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            var SumLen = 0;
            for (int i = 0; i < Ar.Ar.Length; i++)
            {
                var CurrentAr = Ar.Ar[i];
                SumLen += CurrentAr.To - CurrentAr.From;
            }
            this.AddLength(SumLen);
            shiftEnd(From, (From + SumLen - 1), SumLen);
            SetFromTo(Ar, From);
        }
        protected void AddFromTo((int From, int To, ArrayType[] Ar) Ar, int MaxLen, int From)
        {
            AddFromTo((new (int, int, System.Array)[] { Ar }, MaxLen), From);
        }

        protected static void Copy(
            IArray<ArrayType> sourceArray,
            int sourceIndex,
            IArray<ArrayType> destinationArray,
            int destinationIndex,
            int length)
        {
            destinationArray.SetFromTo(sourceArray.GetFromTo(sourceIndex, length), destinationIndex);
        }

        protected static void Copy(
            IArray<ArrayType> sourceArray,
            int sourceIndex,
            ArrayType[] destinationArray,
            int destinationIndex,
            int length)
        {
            var Ar = sourceArray.GetFromTo(sourceIndex, length);
            for (int i = 0; i < Ar.Ar.Length; i++)
            {
                var ThisAr = Ar.Ar[i];
                System.Array.Copy(ThisAr.Ar, ThisAr.From, destinationArray, destinationIndex, ThisAr.To);
                destinationIndex += ThisAr.To;
            }
        }

        protected static void Copy(
            ArrayType[] sourceArray,
            int sourceIndex,
            IArray<ArrayType> destinationArray,
            int destinationIndex,
            int length)
        {
            destinationArray.SetFromTo((sourceIndex, length, sourceArray), length, destinationIndex);
        }

        protected void CopyTo(int From_Pos, ArrayType[] To, int To_Pos, int Length)
        {
            var Ar = GetFromTo(From_Pos, Length);
            for (int i = 0; i < Ar.Ar.Length; i++)
            {
                var ThisAr = Ar.Ar[i];
                System.Array.Copy(ThisAr.Ar, ThisAr.From, To, To_Pos, ThisAr.To);
                To_Pos += ThisAr.To;
            }
        }
        protected void CopyTo(int From_Pos, IArray<ArrayType> To, int To_Pos, int Length)
        {
            To.SetFromTo(GetFromTo(From_Pos, Length), To_Pos);
        }

        public IEnumerator<ArrayType> GetEnumerator()
        {
            return new MyEnum() { ar = this.GetAllArrays().Ar };
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class MyEnum : IEnumerator<ArrayType>
        {
            private (int From, int To, System.Array Ar)[] _ar;
            public (int From, int To, System.Array Ar)[] ar
            {
                set 
                { 
                    _ar = value;
                    var AR = _ar[0];
                    CurrnetAr = (ArrayType[])AR.Ar;
                    CurrnetLen = AR.To;
                    CurrentPos = AR.From - 1;
                    ArLastPos = _ar.Length - 1;
                }
            }
            private ArrayType[] CurrnetAr;
            private int CurrentPos;
            private int CurrnetLen;
            private int ArPos;
            private int ArLastPos;
            public ArrayType Current => CurrnetAr[CurrentPos];

            object IEnumerator.Current => CurrnetAr[CurrentPos];

            public void Dispose()
            {
                System.GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                CurrentPos++;
                var Result = CurrentPos < CurrnetLen;
                if(Result == false)
                {
                    ArPos++;
                    if (ArPos > ArLastPos)
                        return false;
                    var AR = _ar[ArPos];
                    CurrnetAr = (ArrayType[])AR.Ar;
                    CurrnetLen = AR.To;
                    CurrentPos = AR.From;
                    Result = CurrentPos < CurrnetLen;
                }
                return Result;
            }

            public void Reset()
            {
                ArPos = 0;
                CurrnetAr = (ArrayType[])_ar[0].Ar;
                CurrnetLen = CurrnetAr.Length;
                CurrentPos = -1;
            }
        }

        public virtual void SetAllArrays(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar)
        {
            Clear();
            AddFromTo(Ar, 0);
        }

        public virtual ((int From, int To, System.Array Ar)[] Ar, int MaxLen)
            GetAllArrays()
        {
            var Result = GetFromTo(0,Length);
            return Result;
        }
        public virtual void SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            for (int i=0; i <Ar.Ar.Length;i++)
            {
                var _ThisAr = Ar.Ar[i];
                var ThisAr =(ArrayType[]) _ThisAr.Ar;
                var To = _ThisAr.To;
                for (int j = _ThisAr.From; j < To; j++)
                {
                    this[From] = ThisAr[j];
                }
            }
        }

        public ArrayType[] ToArray()
        {
            var NewAr = new ArrayType[Length];
            CopyTo(0, NewAr, 0, Length);
            return NewAr;
        }

        public static implicit operator ArrayType[](IArray<ArrayType> ar)
        {
            return ar.ToArray();
        }

        public virtual void DeleteByPosition(int Position)
        {
            if (Position == Length - 1)
            {
                DeleteFrom(Position);
                return;
            }
            //System.Array.Copy(ar, 0, ar, 0, Position);
            CopyTo(Position + 1, this, Position, (Length - Position) - 1);
            DeleteFrom(Length - 1);
        }
        public abstract void DeleteFrom(int from);
        public void DeleteFromTo(int from, int To)
        {
            CopyTo(To + 1, this, from, Length - (To + 1));
            DeleteFrom(Length - (To - from + 1));
        }
        public void DeleteTo(int To)
        {
            CopyTo(To + 1, this, 0, Length - (To + 1));
            DeleteFrom(Length - (To + 1));
        }

        public ArrayType Pop()
        {
            var Item = this[Length - 1];
            DeleteFrom(Length - 1);
            return Item;
        }

        public virtual void Insert(ArrayType Value)
        {
            Insert(Value, Length);
        }

        public void Insert(IArray<ArrayType> Values)
        {
            var From = Length;
            AddFromTo(Values.GetFromTo(0, Values.Length), From);
        }
        public void Insert(params ArrayType[] Values)
        {
            var From = Length;
            AddFromTo((0, Values.Length, Values), Values.Length, From);
        }
        public void Insert(IEnumerable<ArrayType> Values)
        {
            var From = Length;
            var Count = Values.Count();
            AddLength(Count);
            var i = From;
            Count = Length;
            var Reader = Values.GetEnumerator();
            Reader.MoveNext();
            while (i < Count)
            {
                this[i] = Reader.Current;
                Reader.MoveNext();
                i++;
            }
            Reader.Dispose();
        }
        public void Insert(ArrayType[] Values, int From)
        {
            var ArLen = Length;
            AddFromTo((0, Values.Length, Values), Values.Length, From);
        }
        public void Insert(IEnumerable<ArrayType> Values, int From)
        {
            var ArLen = Length;
            var Count = Values.Count();
            AddLength(Count);
            CopyTo(From, this, ArLen + 1, ArLen - From);
            var i = From;
            Count = Length;
            var Reader = Values.GetEnumerator();
            Reader.MoveNext();
            while (i < Count)
            {
                this[i] = Reader.Current;
                Reader.MoveNext();
                i++;
            }
            Reader.Dispose();
        }
        public virtual void Insert(ArrayType Value, int Position)
        {
            AddLength(1);
            if (Position == Length)
                this[Position] = Value;
            else
            {
                SetFromTo(GetFromTo(Position, Length - Position - 1), Position + 1);
                //CopyTo(Position, this, Position + 1, Length - Position - 1);
                this[Position] = Value;
            }
        }

        public void DropFromInsertTo(int From, int To, ArrayType Value)
        {
            if (From < To)
            {
                CopyTo(From + 1, this, From, (To - From));
            }
            else if (From > To)
            {
                CopyTo(To, this, To + 1, (From - To));
            }
            this[To] = Value;
        }
        public void DropFromInsertTo(int From, int To)
        {
            var Value = this[From];
            if (From < To)
            {
                CopyTo(From + 1, this, From, (To - From));
            }
            else if (From > To)
            {
                CopyTo(To, this, To + 1, (From - To));
            }
            this[To] = Value;
        }

        System.Array IArray.ToArray()
        {
            return ToArray();
        }

        void IArray.Insert(System.Array Array)
        {
            Insert((ArrayType[])Array);
        }


        public t[] Browse<t>(Func<ArrayType, t> Selector)
        {
            var Result = new t[Length];
            for (int i = 0; i < Length; i++)
                Result[i] = Selector(this[i]);
            return Result;
        }
        public ArrayType[] Browse(Func<ArrayType, bool> Selector)
        {
            var Result = new ArrayType[Length];
            var Count = 0;
            foreach (var Value in this)
            {
                if (Selector(Value))
                {
                    Result[Count] = Value;
                    Count++;
                }
            }
            System.Array.Resize(ref Result, Count);
            return Result;
        }
        public void Browse(Action<ArrayType> Visitor)
        {
            foreach (var Value in this)
                Visitor(Value);
        }

        public int Count(Func<ArrayType, bool> Selector)
        {
            var Count = 0;
            foreach (var Value in this)
                if (Selector(Value))
                    Count++;
            return Count;
        }

        public void shiftEnd(int Count) =>
            shiftEnd(0, Length - 1, Count);
        public void shiftBegin(int Count) =>
            shiftBegin(0, Length - 1, Count);

        public void shiftRollEnd(int Count) =>
            shiftRollEnd(0, Length - 1, Count);
        public void shiftRollBegin(int Count) =>
            shiftRollBegin(0, Length - 1, Count);

        public ArrayType[] shiftExtraEnd(int Count) =>
            shiftExtraEnd(0, Length - 1, Count);
        public ArrayType[] shiftExtraBegin(int Count) =>
            shiftExtraBegin(0, Length - 1, Count);

        public void shiftEnd(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
                return;
            _shiftEnd(From, Count, ArLen);
        }
        private void _shiftEnd(int From, int Count, int ArLen)
        {
            Copy(this, From, this, From + Count, ArLen - Count);
        }

        public void shiftBegin(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
                return;
            _shiftBegin(From, Count, ArLen);
        }
        private void _shiftBegin(int From, int Count, int ArLen)
        {
            Copy(this, From + Count, this, From, ArLen - Count);
        }


        public ArrayType[] shiftExtraEnd(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
            {
                var Roll = new ArrayType[Count];
                Copy(this, From, Roll, 0, Count);
                return Roll;
            }
            return _shiftExtraEnd(From, To, Count, ArLen);
        }
        private ArrayType[] _shiftExtraEnd(int From, int To, int Count, int ArLen)
        {
            var Roll = new ArrayType[Count];
            Copy(this, To + 1 - Count, Roll, 0, Count);
            _shiftEnd(From, Count, ArLen);
            return Roll;
        }

        public ArrayType[] shiftExtraBegin(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
            {
                var Roll = new ArrayType[Count];
                Copy(this, From, Roll, 0, Count);
                return Roll;
            }
            return _shiftExtraBegin(From, To, Count, ArLen);
        }
        private ArrayType[] _shiftExtraBegin(int From, int To, int Count, int ArLen)
        {
            var Roll = new ArrayType[Count];
            Copy(this, From, Roll, 0, Count);
            _shiftBegin(From, Count, ArLen);
            return Roll;
        }

        public void shiftRollEnd(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
                return;
            var Roll = _shiftExtraEnd(From, To, Count, ArLen);
            Copy(Roll, 0, this, From, Count);
        }
        public void shiftRollBegin(int From, int To, int Count)
        {
            var ArLen = (To - From) + 1;
            if (Count == ArLen)
                return;
            var Roll = _shiftExtraBegin(From, To, Count, ArLen);
            To = To + 1 - Count;
            Copy(Roll, 0, this, To, Count);
        }
    }

    public abstract class IArray<ArrayType, OwnerType> :
        IArray<ArrayType>
        where OwnerType : IArray<ArrayType, OwnerType>
    {
        protected abstract OwnerType MakeSameNew();

        public OwnerType Copy()
        {
            var NewAr = MakeSameNew();
            NewAr.AddFromTo(GetFromTo(0, Length), 0);
            return NewAr;
        }

        public OwnerType From(int from)
        {
            var Result = MakeSameNew();
            Result.AddFromTo(GetFromTo(from,Length-from),0);
            return Result;
        }
        public OwnerType To(int to)
        {
            var Result = MakeSameNew();
            Result.AddFromTo(GetFromTo(0,to+1), 0);
            return Result;
        }

        public OwnerType FromTo(int from, int to)
        {
            var Result = MakeSameNew();
            Result.AddFromTo(GetFromTo(from, to + 1), 0);
            return Result;
        }

        public OwnerType PopFrom(int From)
        {
#if DEBUG
            if (From > this.Length)
                throw new Exception("From Bigger Than Len");
#endif
            var Result = this.From(From);
            DeleteFrom(From);
            return Result;
        }
        public OwnerType PopTo(int to)
        {
            var Result = this.To(to);
            DeleteTo(to);
            return Result;
        }
        public OwnerType PopFromTo(int From, int To)
        {
            var Result = this.From(From).To(To);
            DeleteFromTo(From, To);
            return Result;
        }

        public OwnerType InsertResult(ArrayType Value)
        {
            Insert(Value, Length);
            return (OwnerType) this;
        }

        public OwnerType InsertResult(IArray<ArrayType> Values)
        {
            var From = Length;
            AddFromTo(Values.GetFromTo(0, Values.Length), From);
            return (OwnerType)this;
        }
        public OwnerType InsertResult(params IArray<ArrayType>[] Values)
        {
            var Result = this;
            for (int i = 0; i < Values.Length; i++)
                this.Insert(Values[i]);
            return (OwnerType)this;
        }
        public OwnerType InsertResult(IEnumerable<ArrayType> Values)
        {
            this.Insert(Values);
            return (OwnerType)this;
        }
    }
}