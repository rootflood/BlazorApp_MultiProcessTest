using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Base
{
    public class ValueOf<ValueType>
    {
        public ValueType Value;
        public static implicit operator ValueOf<ValueType>(ValueType Value)
        {
            return new ValueOf<ValueType>() { Value = Value };
        }

        public static implicit operator ValueType(ValueOf<ValueType> Value)
        {
            if (Value == null)
                return default;
            return Value.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

    }
    public static class Ex
    {
        public static t[] Browse<t, ArrayType>(
            this IEnumerable<ArrayType> IE,
            Func<ArrayType, t> Selector)
        {
            var Result = new t[IE.Count()];
            var E = IE.GetEnumerator();
            for (int i = 0; E.MoveNext(); i++)
                Result[i] = Selector(E.Current);
            return Result;
        }
        public static ArrayType[] Browse<ArrayType>(
            this IEnumerable<ArrayType> IE,
            Func<ArrayType, bool> Selector)
        {
            var Result = new ArrayType[IE.Count()];
            var Count = 0;
            foreach (var Value in IE)
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
        public static void Browse<ArrayType>(
            this IEnumerable<ArrayType> IE, 
            Action<ArrayType> Visitor)
        {
            foreach (var Value in IE)
                Visitor(Value);
        }

        public static ValueOf<r> Result<t, r>(
            this IEnumerable<t> IE,
            Func<t, r> GetValue,
            Func<(r Current, r Before), r> MakeResult)
        {
            var Browser = IE.GetEnumerator();
            if (Browser.MoveNext() == false)
                return null;

            var Value = GetValue(Browser.Current);
            while (Browser.MoveNext())
            {
                Value = MakeResult((GetValue(Browser.Current), Value));
            }
            return new ValueOf<r>() { Value = Value };
        }
        public static ValueOf<r> Result<t, r>(
            this IEnumerable<t> IE,
            Func<t, r> GetValue,
            Func<(r Current, r Before), bool> MakeResult)
        {
            var Browser = IE.GetEnumerator();
            if (Browser.MoveNext()==false)
                return null;
            var Value = GetValue(Browser.Current);
            while(Browser.MoveNext())
            {
                var Current = GetValue(Browser.Current);
                if (MakeResult((Current, Value)))
                    Value = Current;
            }

            return new ValueOf<r>() { Value = Value };
        }

//        public static ValueOf<r> Average<t,r>(
//            this IEnumerable<t> IE,
//            Func<t, r> selector)
//        {
//            var Result = IE.Result((c) =>
//                (dynamic)selector(c), (c) => (c.Before + c.Current) / 2);
//            if (Result == null)
//                return null;
//            else
//            {


//                return new ValueOf<r>() { Value = Result.Value };
//            }
//        }
//        public static ValueOf<r> Max<t, r>(
//            this IEnumerable<t> IE,
//            Func<t, r> selector) 
//        {
//            var Result = IE.Result((c) => 
//                (dynamic)selector(c), (c) =>(bool)(c.Current > c.Before));
//            if (Result == null)
//                return null;
//            else
//            {
//#if DEBUG
//                var DRS = Enumerable.Max(IE, (c) => float.Parse(selector(c).ToString()));
//                if (Math.Round(DRS, 5) != Math.Round(float.Parse(Result.Value.ToString()), 5))
//                    throw new Exception();
//#endif
//                return new ValueOf<r>() { Value = Result.Value };
//            }
//        }
//        public static ValueOf<r> Min<t,r>(
//            this IEnumerable<t> IE,
//            Func<t,r> selector)
//        {
//            var Result = IE.Result((c) =>
//                (dynamic)selector(c), (c) => (bool)(c.Current < c.Before));
//            if (Result == null)
//                return null;
//            else
//            {
//#if DEBUG
//                var DRS = Enumerable.Min(IE, (c) => float.Parse(selector(c).ToString()));
//                if (Math.Round(DRS,5) != Math.Round(float.Parse(Result.Value.ToString()),5))
//                    throw new Exception();
//#endif
//                return new ValueOf<r>() { Value = Result.Value };
//            }
//        }

//        public static ValueOf<r> Sum<t, r>(
//            this IEnumerable<t> IE,
//            Func<t, r> selector)
//        {
//            var Browser = IE.GetEnumerator();
//            if (Browser.MoveNext() == false)
//                return null;
//            dynamic Value = selector(Browser.Current);
//            while (Browser.MoveNext())
//                Value += selector(Browser.Current);

//            return new ValueOf<r>() { Value = Value };
//        }

//        public static int Count<ArrayType>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, bool> Selector)
//        {
//            var Count = 0;
//            foreach (var Value in IE)
//                if (Selector(Value))
//                    Count++;
//            return Count;
//        }

//        public static ValueOf<ArrayType> First<ArrayType>(
//            this IEnumerable<ArrayType> IE)
//        {
//            var Browser = IE.GetEnumerator();
//            if (Browser.MoveNext() == false)
//                return null;
//            return new ValueOf<ArrayType>() { Value = Browser.Current };
//        }

//        public static ValueOf<ArrayType> First<ArrayType>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, bool> predicate)
//        {
//            var Browser = IE.GetEnumerator();
//            if (Browser.MoveNext() == false)
//                return null;
//            while (Browser.MoveNext())
//            {
//                var Current = Browser.Current;
//                if (predicate(Current))
//                    return new ValueOf<ArrayType>() { Value = Current };
//            }
//            return null;
//        }
//        public static ValueOf<ArrayType> Last<ArrayType>(
//            this IEnumerable<ArrayType> IE)
//        {
//            var Browser = IE.GetEnumerator();
//            if (Browser.MoveNext() == false)
//                return null;
//            var Current = Browser.Current;
//            while (Browser.MoveNext())
//                Current = Browser.Current;
//            return new ValueOf<ArrayType>() { Value = Current };
//        }

//        public static IEnumerable<ArrayType> OrderBy<ArrayType, t>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, t> Selector) =>
//                Enumerable.OrderBy(IE, Selector);
//        public static IEnumerable<ArrayType> OrderByDescending<ArrayType, t>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, t> Selector) =>
//                Enumerable.OrderByDescending(IE,Selector);

//        public static ArrayType[] ToArray<ArrayType>(
//            this IEnumerable<ArrayType> IE) =>
//                Enumerable.ToArray(IE);
//        public static IEnumerable<ArrayType> Skip<ArrayType>(
//            this IEnumerable<ArrayType> IE,int Len) =>
//                Enumerable.Skip(IE,Len);
//        public static IEnumerable<ArrayType> Take<ArrayType>(
//            this IEnumerable<ArrayType> IE, int Len) =>
//                Enumerable.Take(IE, Len);
//        public static IEnumerable<ArrayType> TakeWhile<ArrayType>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, bool> predicate) =>
//                Enumerable.TakeWhile(IE, predicate);
//        public static IEnumerable<ArrayType> SkipWhile<ArrayType>(
//            this IEnumerable<ArrayType> IE,
//            Func<ArrayType, bool> predicate) =>
//                Enumerable.SkipWhile(IE,predicate);
    }

    public abstract partial class IArray<ArrayType>
    {
        public ValueOf<ArrayType> First()
        {
            if (Length == 0)
                return null;
            return new ValueOf<ArrayType>() { Value = this[0] };
        }
        
        public ValueOf<ArrayType> First(Func<ArrayType, bool> predicate)
        {
            for(int i=0;i<Length;i++)
            {
                var Value = this[i];
                if (predicate(Value))
                    return new ValueOf<ArrayType>() { Value = Value};
            }

            return null;
        }
        public ValueOf<ArrayType> Last()
        {
            if (Length == 0)
                return null;
            return new ValueOf<ArrayType>() { Value = this[Length-1] };
        }

        public ValueOf<ArrayType> Last(Func<ArrayType, bool> predicate)
        {
            for (int i = Length-1; i >-1; i--)
            {
                var Value = this[i];
                if (predicate(Value))
                    return new ValueOf<ArrayType>() { Value = Value };
            }

            return null;
        }
    }
}
