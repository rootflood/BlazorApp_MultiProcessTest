using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.DynamicSize
{
    public static class ArrayMaker
    {
        public static Array<ArrayType> Make<ArrayType>(ArrayType ItemType) =>
            new Array<ArrayType>();
    }

    public abstract class ArrayBase<ArrayType,OwnerType>:
        Base.IArray<ArrayType,OwnerType>
        where OwnerType: ArrayBase<ArrayType, OwnerType>,new()
    {
        public ArrayType[] ar;
        public int MinLen;
        public int MaxLen;
        public int MinCount;

        public ArrayBase() : this(500) { }

        public ArrayBase(int MinCount)
        {
            this.SetMyOptions(MinCount);
        }

        public ArrayBase(ArrayType[] ar,int MinCount)
        {
            Length = ar.Length;
            this.ar=ar;
            this.SetMyOptions(MinCount);
        }

        private void SetMyOptions(int value)
        {
            this.MinCount = value;
            if (ar == null)
            {
                this.MinLen = Length - MinCount;
                this.MaxLen = Length + MinCount;
                this.ar = new ArrayType[MaxLen];
            }
            else
            {
                this.MinLen = Length;
                this.MaxLen = Length;
            }
        }
        public override object MyOptions { 
            get => MinCount;
            set=> SetMyOptions((int) value);
        }


        public override ArrayType this[int Pos]
        {
            get => ar[Pos];
            set => ar[Pos] = value;
        }

        public override void DeleteFrom(int from)
        {
            Length = from;
            if (Length < MinLen)
            {
                MaxLen = Length + MinCount;
                MinLen = Length - MinCount;
                System.Array.Resize(ref ar, MaxLen);
            }
        }
        protected override void AddLength(int Count)
        {
            Length = Length + Count;
            if (Length > MaxLen)
            {
                MaxLen = Length + MinCount;
                MinLen = Length - MinCount;
                System.Array.Resize(ref ar, MaxLen);
            }
        }

        protected override void 
            AddFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            var OldCount = Length;
            var NewCount = 0;
            for (int i = 0; i < Ar.Ar.Length; i++)
            {
                NewCount += Ar.Ar[i].To;
            }
            AddLength(NewCount);
            CopyTo(From, this, From + NewCount, (OldCount - From));
            SetFromTo(Ar, From);
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen) 
            GetFromTo(int From, int To)
        {
            return (new (int, int, System.Array)[] { (From, To, ar) },To);
        }
        public override void 
            SetFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            var To = Ar.Ar.Length;
            for (int i=0;i<To;i++)
            {
                var Values = Ar.Ar[i];
                System.Array.Copy(Values.Ar, Values.From, ar, From, Values.To);
                From += Values.To;
            }
        }

        public new Array<t> Browse<t>(Func<ArrayType, t> Selector)
        {
            var Result = new Array<t>(this.MinCount);
            Result.Insert(base.Browse(Selector));
            return Result;
        }
        public new Array<ArrayType> Browse(Func<ArrayType, bool> Selector)
        {
            var Result = new Array<ArrayType>(this.MinCount);
            Result.Insert(base.Browse(Selector));
            return Result;
        }

        public static implicit operator ArrayType[](ArrayBase<ArrayType, OwnerType> ar)
        {
            var NewAr = new ArrayType[ar.Length];
            System.Array.Copy(ar.ar, 0, NewAr, 0, ar.Length);
            return NewAr;
        }
    }

    public class Array<ArrayType>:
        ArrayBase<ArrayType, Array<ArrayType>>
    {
        public Array() :
            base(500)
        {}
        public Array(int MinCount=500):
            base(MinCount)
        {}
        public Array(ArrayType[] ar, int MinCount = 500) :
            base(ar,MinCount)
        {}

        protected override Array<ArrayType> MakeSameNew()
        {
            return new Array<ArrayType>(MinCount);
        }
    }
}