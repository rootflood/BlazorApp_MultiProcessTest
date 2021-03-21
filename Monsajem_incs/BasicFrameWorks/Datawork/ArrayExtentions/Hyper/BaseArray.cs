using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Hyper
{
    public abstract class BaseArray<ArrayType, OwnerType> :
        Base.IArray<ArrayType, OwnerType>
        where OwnerType:BaseArray<ArrayType,OwnerType>
    {
        public class ArrayInstance :
            IComparable<ArrayInstance>
        {
            public int FromPos;
            public DynamicSize.Array<ArrayType> ar;
            public int CompareTo(ArrayInstance other)
            {
                return this.FromPos - other.FromPos;
            }
        }
        public DynamicSize.SortedArray<ArrayInstance> ar;
        internal int MinCount;
        private int MaxLen;
        private int MaxLen_Div2;

        public BaseArray() : this(500) { }

        public BaseArray(int MinCount = 500)
        {
            this.MyOptions = MinCount;
        }
        public BaseArray(ArrayType[] ar, int MinCount = 500) : this(MinCount)
        {
            Insert(ar);
        }

        public override object MyOptions { get => MinCount;
            set 
            {
                this.MinCount =(int) value;
                this.MaxLen = MinCount - 1;
                this.MaxLen_Div2 = MaxLen / 2;

                if(this.ar==null)
                {
                    this.ar = new DynamicSize.SortedArray<ArrayInstance>();
                    ar.Insert(new ArrayInstance()
                    {
                        ar = new DynamicSize.Array<ArrayType>(MinCount),
                        FromPos = 0
                    }, 0);
                }
            } 
        }

        public override ArrayType this[int Pos]
        {
            get
            {
                ArrayInstance Myar;
                var arInfo = ar.BinarySearch(new ArrayInstance() { FromPos = Pos });
                var Index = arInfo.Index;
                if (Index < 0)
                {
                    Index = (Index * -1) - 2;
                    Myar = ar[Index];
                    return Myar.ar[Pos - Myar.FromPos];
                }

                Myar = arInfo.Value;
                return Myar.ar[Pos - Myar.FromPos];
            }
            set
            {
                ArrayInstance Myar;
                var arInfo = ar.BinarySearch(new ArrayInstance() { FromPos = Pos });
                var Index = arInfo.Index;
                if (Index < 0)
                {
                    Index = (Index * -1) - 2;
                    Myar = ar[Index];
                    Myar.ar[Pos - Myar.FromPos]=value;
                    return;
                }

                Myar = arInfo.Value;
                Myar.ar[Pos - Myar.FromPos] = value;
                return;
            }
        }
        private void Optimization(DynamicSize.Array<ArrayInstance> ar)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                i+=Optimization(ar, i);
            }
        }
        private int Optimization(DynamicSize.Array<ArrayInstance> ar, int Pos)
        {
            var OldAr = ar[Pos];
            if (OldAr.ar.Length > MaxLen)
            {
                var NewAr = OldAr.ar.PopFrom(MaxLen_Div2);
                ar.Insert(new ArrayInstance()
                {
                    FromPos = OldAr.FromPos + OldAr.ar.Length,
                    ar = new DynamicSize.Array<ArrayType>(NewAr, MinCount)
                },
                Pos + 1);
                return 1;
            }
            else if (OldAr.ar.Length == 0)
            {
                if (ar.Length > 1)
                {
                    
                    ar.DeleteByPosition(Pos);
                    return -1;
                }
            }
            else if (OldAr.ar.Length < MaxLen_Div2)
            {
                if (Pos == 0)
                {
                    if (ar.Length > 1)
                    {
                        var NextAr = ar[1];
                        if ((NextAr.ar.Length + OldAr.ar.Length) < (MaxLen_Div2))
                        {
                            NextAr.ar.Insert(OldAr.ar.ToArray(), 0);
                            NextAr.FromPos = 0;
                            ar.DeleteByPosition(0);
                            return -1;
                        }
                    }
                }
                else if (Pos == ar.Length - 1)
                {
                    var BeforeAr = ar[Pos - 1];
                    if ((BeforeAr.ar.Length + OldAr.ar.Length) < (MaxLen_Div2))
                    {
                        BeforeAr.ar.Insert(OldAr.ar.ToArray());
                        ar.DeleteByPosition(Pos);
                        return -1;
                    }
                }
                else
                {
                    var NextAr = ar[Pos + 1];
                    if ((NextAr.ar.Length + OldAr.ar.Length) < (MaxLen_Div2))
                    {
                        NextAr.ar.Insert(OldAr.ar.ToArray(), 0);
                        NextAr.FromPos = OldAr.FromPos;
                        ar.DeleteByPosition(Pos);
                        return -1;
                    }
                    else
                    {
                        var BeforeAr = ar[Pos - 1];
                        if ((BeforeAr.ar.Length + OldAr.ar.Length) < (MaxLen_Div2))
                        {
                            BeforeAr.ar.Insert(OldAr.ar.ToArray());
                            ar.DeleteByPosition(Pos);
                            return -1;
                        }
                    }
                }
            }
            return 0;
        }

        public override void Insert(ArrayType Value, int Position)
        {

            var arInfo = ar.BinarySearch(new ArrayInstance() { FromPos = Position });
            var arPos = arInfo.Index;
            ArrayInstance MyAr;
            if (arPos < 0)
            {
                arPos = (arPos * -1) - 2;
                MyAr = ar[arPos];
            }
            else
                MyAr = arInfo.Value;
            MyAr.ar.Insert(Value, Position - MyAr.FromPos);
            for (int i = arPos + 1; i < ar.Length; i++)
                ar[i].FromPos += 1;
            Optimization(ar, arPos);
            Length++;
        }

        public override void DeleteByPosition(int Position)
        {
            var arPos = ar.BinarySearch(new ArrayInstance() { FromPos = Position }).Index;
            if (arPos < 0)
            {
                arPos = (arPos * -1) - 2;
            }
            var MyAr = ar[arPos];
            MyAr.ar.DeleteByPosition(Position - MyAr.FromPos);
            for (int i = arPos + 1; i < ar.Length; i++)
                ar[i].FromPos -= 1;
            Optimization(ar, arPos);
            Length--;
        }

        protected override void AddLength(int Count)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFrom(int from)
        {
            var arPos = ar.BinarySearch(new ArrayInstance() { FromPos = from }).Index;
            if (arPos < 0)
            {
                arPos = (arPos * -1) - 2;
                var MyAr = ar[arPos];
                MyAr.ar.DeleteFrom(from - MyAr.FromPos);
            }
            else
            {
                if (arPos == 0)
                    ar[0].ar.DeleteFrom(0);
                else
                    ar.DeleteFrom(arPos);
            }
            Length = from;
        }

        public override void SetAllArrays(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar)
        {
            ar.Clear();
            var From = 0;
            ArrayInstance[] NewArs = new ArrayInstance[Ar.Ar.Length];
            for (int i = 0; i < Ar.Ar.Length; i++)
            {
                var ThisAr = Ar.Ar[i];
                var NewAr = new ArrayInstance()
                {
                    ar = new DynamicSize.Array<ArrayType>((ArrayType[])ThisAr.Ar, MinCount),
                    FromPos = From
                };
                NewArs[i] = NewAr;
                From += ThisAr.To;
            }
            ar = new DynamicSize.SortedArray<ArrayInstance>(NewArs);
        }

        protected override void
            AddFromTo(((int From, int To, System.Array Ar)[] Ar, int MaxLen) Ar, int From)
        {
            var arPos = this.ar.BinarySearch(new ArrayInstance() { FromPos = From }).Index;
            if (arPos < 0)
            {
                arPos = (arPos * -1) - 2;
                var FromAr = ar[arPos];
                FromAr = new ArrayInstance()
                {
                    ar = FromAr.ar.PopFrom(From - FromAr.FromPos)
                };
                arPos += 1;
                ar.Insert(new ArrayInstance()
                {
                    FromPos = From,
                    ar = FromAr.ar
                }, arPos);
            }

            var Count = 0;
            var Instances = new ArrayInstance[Ar.Ar.Length];
            for (int i = 0; i < Instances.Length; i++)
            {
                var ThisAr = Ar.Ar[i];
                Instances[i] = new ArrayInstance()
                {
                    ar = new DynamicSize.Array<ArrayType>((ArrayType[])ThisAr.Ar, MinCount),
                    FromPos = From
                };
                Count += ThisAr.To;
            }

            var Opt = new DynamicSize.Array<ArrayInstance>(Instances, Instances.Length);
            Optimization(Opt);
            Instances = Opt.ToArray();

            ar.Insert(Instances, arPos);
            for (int i = arPos + Instances.Length; i < ar.Length; i++)
            {
                ar[i].FromPos += Count;
            }
            var Pos = Optimization(ar,arPos);
            Pos = (arPos + Opt.Length) + Pos;
            Optimization(ar,Pos);
            this.Length += Count;
        }

        public override ((int From, int To, System.Array Ar)[] Ar, int MaxLen)
            GetFromTo(int From, int To)
        {
            var arPos = ar.BinarySearch(new ArrayInstance() { FromPos = From }).Index;
            if (arPos < 0)
            {
                arPos = (arPos * -1) - 2;
            }
            var Result = new (int From, int To, System.Array Ar)[ar.Length - arPos];
            if (ar.Length > 0)
                From = ar[0].FromPos - From;
            
            for (int i = 0; i < Result.Length; i++)
            {
                var MyAr = ar[arPos+i];
                To -= MyAr.ar.Length;
                if(To > 0)
                    Result[i] = (From, MyAr.ar.Length, MyAr.ar);
                else
                {
                    Result[i] = (From, MyAr.ar.Length + To, MyAr.ar);
                    System.Array.Resize(ref Result, i + 1);
                    break;
                }
                From = 0;
            }

            return (Result, MinCount);
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
    }
}