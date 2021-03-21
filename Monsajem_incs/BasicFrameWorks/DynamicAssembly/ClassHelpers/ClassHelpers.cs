using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Runtime.CompilerServices;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.DynamicAssembly
{
    public class AutoRun : Attribute
    {

    }

    public static class ClassHelpers
    {
        private static MethodInfo[] GetMothods(Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<MethodInfo, bool> filter = null)
        {
            var fiels = typeToReflect.GetMethods(bindingFlags);
            if (filter != null)
                fiels = fiels.Where((c) => filter(c)).ToArray();
            if (typeToReflect.BaseType != null)
            {
                Insert(ref fiels, GetMothods(typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate));
            }
            return fiels;
        }


        public static void AutoRuns<t>(this t Class)
        {
            var Methods = GetMothods(typeof(t)).
                Where((c) => c.CustomAttributes.Where((a) => a.AttributeType == typeof(AutoRun)).Count() > 0);

            foreach(var method in Methods)
            {
                method.Invoke(Class, null);
            }
        }

        internal static FieldInfo[] _GetFields(Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            var fiels = typeToReflect.GetFields(bindingFlags);
            if (filter != null)
                fiels = fiels.Where((c) => filter(c)).ToArray();
            if (typeToReflect.BaseType != null)
            {
                Insert(ref fiels, _GetFields(typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate));
            }
            return fiels;
        }
        public static FieldInfo[] GetFields(Type typeToReflect)
        {
            return _GetFields(typeToReflect);
        }

        //internal static FieldInfo[] GetMethods(Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        //{
        //    var fiels = typeToReflect.GetMethods(bindingFlags);
        //    if (filter != null)
        //        fiels = fiels.Where((c) => filter(c)).ToArray();
        //    if (typeToReflect.BaseType != null)
        //    {
        //        Insert(ref fiels, GetFields(typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate));
        //    }
        //    return fiels;
        //}
        //public static FieldInfo[] GetFields(Type typeToReflect)
        //{
        //    return GetFields(typeToReflect);
        //}
    }
}
