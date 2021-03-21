using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using Monsajem_Incs.ArrayExtentions;

namespace Monsajem_Incs.Pattern
{
    public class Function<t>
    {
        public object Info;
        private Function<t>[] Subs = new Function<t>[0];
        private Func<t[], int, (bool InRange, int PutLen)> IsInRange;

        public Function(Func<t[], int, bool> IsInRange) :
            this((c, p) =>
            {
                if (IsInRange(c, p))
                    return (true, 1);
                else
                    return (false, 0);
            })
        { }

        public Function(bool IsMatch, params t[] Keys) :
            this((c, p) =>
            {
                if (IsMatch == Keys.Contains(c[p]))
                    return (true, 1);
                return (false, 0);
            })
        { }
        public Function(int AlwaysPut, bool IsMatch, params t[] Keys) :
            this((c, p) =>
            {
                if (IsMatch == Keys.Contains(c[p]))
                    return (true, 1);
                return (false, 0);
            })
        { }

        public Function(Func<t[], int, (bool InRange, int PutLen)> IsInRange)
        {
            this.IsInRange = IsInRange;
        }

        public void AddSub(params Function<t>[] Func)
        {
            Insert(ref Subs, Func);
        }

        public BrowsedFunction<t> Browse(t[] Function)
        {
            var Pos = 0;
            var Len = Function.Length;
            return _Browse(ref Function, ref Pos, Len);
        }

        private void InsertValue(BrowsedFunction<t> Browsed, t[] Function, ref int Pos, int Len)
        {
            if (Len == 0)
                return;
            var LastPart = Browsed.SubFunctions.LastOrDefault();
            var Values = Function.From(Pos).To(Len - 1);
            if (LastPart == null || LastPart.IsSub == true)
                Insert(ref Browsed.SubFunctions, new BrowsedFunction<t>() { Info = Info, Values = Values });
            else
                Insert(ref LastPart.Values, Values);
            Pos += Len;
        }

        private BrowsedFunction<t> _Browse(ref t[] Function, ref int Pos, int Len)
        {
            var Result = new BrowsedFunction<t>() { Info = Info };
            while (Pos < Len)
            {
                var IsIn = IsInRange(Function, Pos);
                if (IsIn.InRange)
                {
                    InsertValue(Result, Function, ref Pos, IsIn.PutLen);
                }
                else
                {
                    var ISEnd = true;
                    foreach (var Sub in Subs)
                        if (Sub.IsInRange(Function, Pos).Item1)
                        {
                            ISEnd = false;
                            Insert(ref Result.SubFunctions, Sub._Browse(ref Function, ref Pos, Len));
                        }
                    if (ISEnd)
                    {
                        InsertValue(Result, Function, ref Pos, IsIn.PutLen);
                        return Result;
                    }

                }
            }
            return Result;
        }
    }

    public class BrowsedFunction<t>
    {
        public object Info;
        public t[] Values;
        public BrowsedFunction<t>[] SubFunctions = new BrowsedFunction<t>[0];

        public bool IsSub { get => Values == null; }

        public BrowsedFunction<t> this[int Pos]
        {
            get => SubFunctions[Pos];
        }

        public t[] Compile
        {
            get
            {
                if (IsSub)
                {
                    var Result = new t[0];
                    foreach (var Sub in SubFunctions)
                        Insert(ref Result, Sub.Compile);
                    return Result;
                }
                else
                    return Values;
            }

        }

    }
}
