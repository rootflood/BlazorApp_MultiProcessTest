using Monsajem_Incs.Pattern;
using System;
using System.Linq;
using System.Reflection;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs
{
    namespace Assembly
    {
        public class Assembly
        {
            public static System.Reflection.Assembly[] AllAppAssemblies{get=>_AllAppAssemblies;}
            internal static System.Reflection.Assembly[] _AllAppAssemblies=
                ((Func<System.Reflection.Assembly[]>)(()=> {
                    var Asms = AppDomain.CurrentDomain.GetAssemblies();
                    if (System.Reflection.Assembly.GetExecutingAssembly()!=null)
                        Insert(ref Asms, System.Reflection.Assembly.GetExecutingAssembly());
                    if (System.Reflection.Assembly.GetCallingAssembly() != null)
                        Insert(ref Asms, System.Reflection.Assembly.GetCallingAssembly());
                    if (System.Reflection.Assembly.GetEntryAssembly() != null)
                        Insert(ref Asms, System.Reflection.Assembly.GetEntryAssembly());
                    Asms = Asms.GroupBy((c) => c.FullName).Select((c) => c.First()).ToArray();
                    var AllAsm = new System.Reflection.Assembly[0];
                    foreach (var Asm in Asms)
                    {
                        Insert(ref AllAsm, Asm);
                        var InnerAsms = Asm.GetReferencedAssemblies();
                        foreach (var InnerAsm in InnerAsms)
                        {
                            try
                            {
                                Insert(ref AllAsm, System.Reflection.Assembly.Load(InnerAsm));
                            }
                            catch { }
                        }
                    }
                    AllAsm = AllAsm.GroupBy((c) => c.FullName).Select((c) => c.First()).ToArray();
                    return AllAsm;
                }))();

            public static void TryLoadAssembely(string Name)
            {
                try
                {
                    Insert(ref _AllAppAssemblies, System.Reflection.Assembly.Load(Name));
                    _AllAppAssemblies =_AllAppAssemblies.GroupBy((c) => c.FullName).Select((c) => c.First()).ToArray();
                }
                catch { }
            }
        }
    }
    internal static class Shared
    {
        internal static string MidName(this Type Type)
        {
            return Type.ToString();
        }

        private static Array.DynamicSize.SortedArray<TypeHolder> Types = new Array.DynamicSize.SortedArray<TypeHolder>();
        internal static Type GetTypeByName(this string TypeName)
        {
            var Type = Types.BinarySearch(new TypeHolder(TypeName)).Value?.Type;
            if (Type != null)
                return Type;
            var Type_P = new Function<char>((c, p) => c[p] != '[' && c[p] != ']' && c[p] != ',')
            { Info = "Type" };
            var Next_P = new Function<char>((c, p) =>
                c[p] == ',')
            { Info = "Next" };
            var Inner_P = new Function<char>((c, p) =>
                (c[p] == '[' || c[p] == ',', 1))
            { Info = "SubType" };

            Type_P.AddSub(Next_P, Inner_P);
            Inner_P.AddSub(Type_P);

            var Browsed = Type_P.Browse(TypeName.ToCharArray());
            if (Browsed.SubFunctions.Length > 1 &&
               Browsed.SubFunctions[1].SubFunctions.Length > 1)
                TypeName = new string(Browsed.SubFunctions[0].Values);
            for (int i = 0; i < Assembly.Assembly._AllAppAssemblies.Length; i++)
            {
                var Asm = Assembly.Assembly._AllAppAssemblies[i];
                Type = Asm.GetType(TypeName);
                if (Type != null)
                {
                    if (Type.IsGenericType)
                    {
                        var Types = new Type[0];
                        var AllSubs = Browsed[1][1].SubFunctions.AsEnumerable();
                        while (AllSubs.Count() > 0)
                        {
                            var Subs = AllSubs.TakeWhile((c) => c.Info != "Next");
                            AllSubs = AllSubs.SkipWhile((c) => c.Info != "Next").Skip(1);
                            var SubType = "";
                            foreach (var Sub in Subs)
                                SubType += new string(Sub.Compile);
                            Insert(ref Types, GetTypeByName(SubType));
                        }
                        Type = Type.MakeGenericType(Types);
                        if (Browsed.SubFunctions.Length > 2)
                        {
                            var Rank = Browsed[2].Compile.Length - 1;
                            if (Rank == 1)
                                Type = Type.MakeArrayType();
                            else
                                Type = Type.MakeArrayType(Rank);
                        }
                    }
                    Types.BinaryInsert(new TypeHolder(Type));
                    return Type;
                }
            }
            throw new TypeLoadException("Type Not Found >> " + TypeName);
        }

        private class TypeHolder:IComparable<TypeHolder>
        {
            public TypeHolder(Type Type):this(Type.ToString())
            {
                this.Type = Type;
            }
            public TypeHolder(string Type)
            {
                this.HashCode = Type.GetHashCode();
            }

            public Type Type;
            public int HashCode;

            public int CompareTo(TypeHolder other)
            {
                return HashCode - other.HashCode;
            }
        }
    }
}
