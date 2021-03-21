using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.ArrayExtentions
{
    public static class ArrayExtentions
    {
        public static void ForEach<t>(
            this t[] array,
            Action<int[]> action)
            => ForEach((System.Array)array, action);
        public static void ForEach(
            this System.Array array,
            Action<int[]> action)
        {
            var Rank = array.Rank;
            var Ends = new int[Rank];
            for (int i = 0; i < Rank; i++)
            {
                Ends[i] = array.GetUpperBound(i) + 1;
            }
            array.ForEach(Ends, action);
        }
        public static void ForEach(
            this System.Array array,
            int[] Ends,
            Action<int[]> action)
        {
            var Rank = Ends.Length;
            var Currents = new int[Rank];

            while (Currents[Currents.Length - 1] < Ends[Ends.Length - 1])
            {
                for (Currents[0] = 0; Currents[0] < Ends[0]; Currents[0]++)
                {
                    action(Currents);
                }
                for (int i = 1; i < Rank; i++)
                {
                    if (Currents[i] < Ends[i])
                    {
                        Currents[i]++;
                        Currents[i - 1] = 0;
                    }
                }
            }
        }

        public static void DeleteByPosition<t>(ref t[] ar, int Position)
        {
            if (Position == ar.Length - 1)
            {
                System.Array.Resize(ref ar, Position);
                return;
            }
            System.Array.Copy(ar, 0, ar, 0, Position);
            System.Array.Copy(ar, Position + 1, ar, Position, (ar.Length - Position) - 1);
            System.Array.Resize(ref ar, ar.Length - 1);
        }

        public static void DeleteFrom<t>(ref t[] ar, int from)
        {
            System.Array.Resize(ref ar, from);
        }
        public static void DeleteFromTo<t>(ref t[] ar, int from,int To)
        {
            System.Array.Copy(ar, To+1, ar, from, ar.Length-(To + 1));
            System.Array.Resize(ref ar, ar.Length - (To - from + 1));
        }
        public static void DeleteTo<t>(ref t[] ar, int To)
        {
            System.Array.Copy(ar, To+1, ar, 0, ar.Length - (To + 1));
            System.Array.Resize(ref ar, ar.Length - (To + 1));
        }

        public static t Pop<t>(ref t[] ar)
        {
            var Item = ar[ar.Length - 1];
            System.Array.Resize(ref ar, ar.Length - 1);
            return Item;
        }
        public static t[] PopFrom<t>(ref t[] ar, int From)
        {
            var Result = ar.From(From);
            DeleteFrom(ref ar, From);
            return Result;
        }
        public static t[] PopTo<t>(ref t[] ar, int to)
        {
            var Result = ar.To(to);
            DeleteTo(ref ar, to);
            return Result;
        }
        public static t[] PopFromTo<t>(ref t[] ar,int From,int To)
        {
            var Result = ar.From(From).To(To);
            DeleteFromTo(ref ar, From, To);
            return Result;
        }

        public static int BinaryDelete<t>(ref t[] ar, t Value)
        {
            var Place = System.Array.BinarySearch(ar, Value);
            if(Place >= 0)
            {
                DeleteByPosition(ref ar,Place);
            }
            return Place;
        }
        public static int BinaryDelete<t>(ref t[] ar, t Value,IComparer comparer)
        {
            var Place = System.Array.BinarySearch(ar, Value, comparer);
            if (Place >= 0)
            {
                DeleteByPosition(ref ar, Place);
            }
            return Place;
        }
        public static int BinaryDelete<t>(ref t[] ar, t Value, IComparer<t> comparer)
        {
            var Place = System.Array.BinarySearch(ar, Value, comparer);
            if (Place >= 0)
            {
                DeleteByPosition(ref ar, Place);
            }
            return Place;
        }

        public static void BinaryDelete<t>(ref t[] ar, IEnumerable<t> Values)
        {
            foreach (var Value in Values)
                BinaryDelete(ref ar, Value);
        }
        public static void BinaryDelete<t>(ref t[] ar,IComparer comparer, IEnumerable<t> Values)
        {
            foreach (var Value in Values)
                BinaryDelete(ref ar, Value,comparer);
        }
        public static void BinaryDelete<t>(ref t[] ar,IComparer<t> comparer, IEnumerable<t> Values)
        {
            foreach (var Value in Values)
                BinaryDelete(ref ar, Value,comparer);
        }

        public static int BinaryInsert<t>(ref t[] ar, t Value)
        {
            var Place = System.Array.BinarySearch(ar, Value);
            if (Place < 0)
                Place = (Place * -1) - 1;
            Insert(ref ar, Value, Place);
            return Place;
        }
        public static int BinaryInsert<t>(ref t[] ar, t Value, IComparer comparer)
        {
            var Place = System.Array.BinarySearch(ar, Value, comparer);
            if (Place < 0)
                Place = (Place * -1) - 1;
            Insert(ref ar, Value, Place);
            return Place;
        }
        public static int BinaryInsert<t>(ref t[] ar, t Value, IComparer<t> comparer)
        {
            var Place = System.Array.BinarySearch(ar, Value, comparer);
            if (Place < 0)
                Place = (Place * -1) - 1;
            Insert(ref ar, Value, Place);
            return Place;
        }


        public static void BinaryInsert<t>(ref t[] ar,params t[] Values)
        {
            for (int i=0;i<Values.Length;i++)
            {
                BinaryInsert(ref ar, Values[i]);
            }
        }
        public static void BinaryInsert<t>(ref t[] ar,IComparer comparer , params t[] Values)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                BinaryInsert(ref ar, Values[i],comparer);
            }
        }
        public static void BinaryInsert<t>(ref t[] ar,IComparer<t> comparer, params t[] Values)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                BinaryInsert(ref ar, Values[i],comparer);
            }
        }

        public static void BinaryInsert<t>(ref t[] ar, IEnumerable<t> Values)
        {
            foreach(var Value in Values)
            {
                BinaryInsert(ref ar, Value);
            }
        }
        public static void BinaryInsert<t>(ref t[] ar,IComparer comparer, IEnumerable<t> Values)
        {
            foreach (var Value in Values)
            {
                BinaryInsert(ref ar, Value,comparer);
            }
        }
        public static void BinaryInsert<t>(ref t[] ar,IComparer<t> comparer, IEnumerable<t> Values)
        {
            foreach (var Value in Values)
            {
                BinaryInsert(ref ar, Value,comparer);
            }
        }

        public static (int OldPlace, int NewPlace) BinaryUpdate<t>(t[] ar, t OldValue, t NewValue)=>
            BinaryUpdate(ar, OldValue, NewValue,
                System.Array.BinarySearch(ar, OldValue),
                System.Array.BinarySearch(ar, NewValue));

        public static (int OldPlace, int NewPlace) BinaryUpdate<t>(
            t[] ar, t OldValue, t NewValue, IComparer<t> Comparer) =>
            BinaryUpdate(ar, OldValue, NewValue,
                System.Array.BinarySearch(ar, OldValue, Comparer),
                System.Array.BinarySearch(ar, NewValue, Comparer));

        private static (int OldPlace, int NewPlace) 
            BinaryUpdate<t>(t[] ar, t OldValue, t NewValue, int OldPlace, int NewPlace)
        {
            if (NewPlace < 0)
                NewPlace = (NewPlace * -1) - 1;

            if (OldPlace == NewPlace)
                ar[OldPlace] = NewValue;
            else if (OldPlace < NewPlace)
            {
                NewPlace -= 1;
                shiftBegin(ar, OldPlace, NewPlace, 1);
                ar[NewPlace] = NewValue;
            }
            else
            {
                shiftEnd(ar, NewPlace, OldPlace, 1);
                ar[NewPlace] = NewValue;
            }
            return (OldPlace, NewPlace);
        }

        public static void Insert<t>(ref t[] ar, t Value)
        {
            System.Array.Resize(ref ar, ar.Length + 1);
            ar[ar.Length - 1] = Value;
        }

        public static void Insert<t>(ref t[] ar,params t[] Values)
        {
            var From = ar.Length;
            System.Array.Resize(ref ar, ar.Length + Values.Length);
            System.Array.Copy(Values, 0, ar, From, Values.Length);
        }
        public static void Insert<t>(ref t[] ar, IEnumerable<t> Values)
        {
            var From = ar.Length;
            var Count = Values.Count();
            System.Array.Resize(ref ar, ar.Length + Count);
            var i = From;
            Count = ar.Length;
            var Reader = Values.GetEnumerator();
            Reader.MoveNext();
            while(i<Count)
            {
                ar[i] = Reader.Current;
                Reader.MoveNext();
                i++;
            }
            Reader.Dispose();
        }
        public static void Insert<t>(ref t[] ar, t[] Values,int From)
        {
            var ArLen = ar.Length;
            System.Array.Resize(ref ar, ar.Length + Values.Length);
            System.Array.Copy(ar, From, ar, ArLen+1, ArLen - From);
            System.Array.Copy(Values, 0, ar, From, Values.Length);
        }
        public static void Insert<t>(ref t[] ar, IEnumerable<t> Values, int From)
        {
            var ArLen = ar.Length;
            var Count = Values.Count();
            System.Array.Resize(ref ar, ar.Length + Count);
            System.Array.Copy(ar, From, ar, ArLen+1, ArLen - From);
            var i = From;
            Count = ar.Length;
            var Reader = Values.GetEnumerator();
            Reader.MoveNext();
            while (i < Count)
            {
                ar[i] = Reader.Current;
                Reader.MoveNext();
                i++;
            }
            Reader.Dispose();
        }
        public static void Insert<t>(ref t[] ar, t Value, int Position)
        {
            System.Array.Resize(ref ar, ar.Length + 1);
            if (Position == ar.Length - 1)
                ar[Position] = Value;
            else
            {
                System.Array.Copy(
                    ar, Position,
                    ar, Position + 1, ar.Length - Position - 1);
                ar[Position] = Value;
            }
        }

        public static void DropFromInsertTo<t>(ref t[] ar, int From, int To, t Value)
        {
            if (From < To)
            {
                System.Array.Copy(ar, From + 1, ar, From, (To - From));
            }
            else if (From > To)
            {
                System.Array.Copy(ar, To, ar, To + 1, (From - To));
            }
            ar[To] = Value;
        }
        public static void DropFromInsertTo<t>(ref t[] ar, int From, int To)
        {
            var Value = ar[From];
            if (From < To)
            {
                System.Array.Copy(ar, From + 1, ar, From, (To - From));
            }
            else if (From > To)
            {
                System.Array.Copy(ar, To, ar, To + 1, (From - To));
            }
            ar[To] = Value;
        }

        public static void shiftEnd<t>(t[] ar, int Len) =>
            shiftEnd(ar, 0, ar.Length - 1, Len);
        public static void shiftBegin<t>(t[] ar, int Len) =>
            shiftBegin(ar, 0, ar.Length - 1, Len);

        public static void shiftRollEnd<t>(t[] ar, int Len) =>
            shiftRollEnd(ar, 0, ar.Length - 1, Len);
        public static void shiftRollBegin<t>(t[] ar, int Len) =>
            shiftRollBegin(ar, 0, ar.Length - 1, Len);

        public static void shiftExtraEnd<t>(t[] ar, int Len) =>
            shiftExtraEnd(ar, 0, ar.Length - 1, Len);
        public static void shiftExtraBegin<t>(t[] ar, int Len) =>
            shiftExtraBegin(ar, 0, ar.Length - 1, Len);

        public static void shiftEnd<t>(t[] ar, int From, int To, int Len)
        {
            System.Array.Copy(ar, From, ar, From + Len, ((To - From) + 1) - Len);
        }
        public static void shiftBegin<t>(t[] ar, int From, int To, int Len)
        {
            System.Array.Copy(ar, From + Len, ar, From, ((To-From)+1) - Len);
        }

        public static void shiftRollEnd<t>(t[] ar, int From, int To, int Len)
        {
            var Roll = shiftExtraEnd(ar, From, To, Len);
            To = To + 1 - Len;
            System.Array.Copy(Roll, 0, ar, From, Len);
        }
        public static void shiftRollBegin<t>(t[] ar, int From, int To, int Len)
        {
            var Roll = shiftExtraBegin(ar, From, To, Len);
            To = To + 1 - Len;
            System.Array.Copy(Roll, 0, ar, To, Len);
        }

        public static t[] shiftExtraEnd<t>(t[] ar, int From, int To, int Len)
        {
            var Roll = new t[Len];
            To = To + 1 - Len;
            System.Array.Copy(ar, To, Roll, 0, Len);
            System.Array.Copy(ar, From, ar, From + Len, To - From);
            return Roll;
        }
        public static t[] shiftExtraBegin<t>(t[] ar, int From, int To, int Len)
        {
            var Roll = new t[Len];
            To = To + 1 - Len;
            System.Array.Copy(ar, From, Roll, 0, Len);
            System.Array.Copy(ar, From + Len, ar, From, To);
            return Roll;
        }
    }
}
