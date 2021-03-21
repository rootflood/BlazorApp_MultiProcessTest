using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Security.Policy;
using System.Security;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.DynamicAssembly
{
    public class FieldControler
    {
        public FieldControler(FieldInfo Field):this(
            Field,
            TypeController.SetValue(Field),
             TypeController.GetValue(Field))
        {}

        public FieldControler(
            FieldInfo Field,
            Action<object, object> SetValue, 
            Func<object, object> GetValue)
        {
            this.Info = Field;
            this.SetValue = SetValue;
            this.GetValue = GetValue;
        }

        public readonly FieldInfo Info;
        public readonly Action<object, object> SetValue;
        public readonly Func<object, object> GetValue;

        public object this[object obj]
        {
            get => GetValue(obj);
        }
        public object this[object obj,object Value]
        {
            set => SetValue(obj,Value);
        }

        public static FieldControler<ParentType, FieldType>
            Make<ParentType, FieldType>(Expression<Func<ParentType, FieldType>> Field)
        {
            return new FieldControler<ParentType, FieldType>(Field);
        }

        public static FieldControler Make(FieldInfo Field)
        {
            return new FieldControler(Field);
        }

        public static FieldInfo[] GetFields(Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            var fiels = typeToReflect.GetFields(bindingFlags);
            if (filter != null)
                fiels = fiels.Where((c) => filter(c)).ToArray();
            if (typeToReflect.BaseType != null)
            {
                Insert(ref fiels, GetFields(typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate));
            }
            return fiels;
        }

        public static FieldControler[] Make(Type Type)
        {
            return Make(GetFields(Type));
        }

        public static FieldControler[] Make(FieldInfo[] Fields)
        {
            var FieldControllers = new FieldControler[Fields.Length];
            for (int i = 0; i < Fields.Length; i++)
            {
                FieldControllers[i] = new FieldControler(Fields[i]);
            }
            return FieldControllers;
        }
    }

    public class FieldControler<ParentType, FieldType>
    {
        private delegate ref FieldType RefReturn(ref ParentType Parent);
        private RefReturn RefValue;
        private Func<ParentType, FieldType> GetValue;
        private Action<object, object> SetValue;
        public readonly FieldInfo Field;
        public FieldControler(Expression<Func<ParentType, FieldType>> Field)
        {
            this.Field = typeof(ParentType).GetField(((MemberExpression)Field.Body).Member.Name);
            this.RefValue = _GetValueRef(this.Field);
            this.GetValue = _GetValue(this.Field);
            this.SetValue = TypeController.SetValue(typeof(ParentType).GetField(((MemberExpression)Field.Body).Member.Name));
        }

        public ref FieldType Value(ref ParentType Parent)
        {
            return ref RefValue(ref Parent);
        }

        public FieldType Value(ParentType Parent,Func<FieldType> Get)
        {
            var Val = Get();
            SetValue(Parent,Val);
            return Val;
        }

        public FieldType Value(ParentType Parent, Func<FieldType,FieldType> Get)
        {
            var Val = Get(GetValue(Parent));
            SetValue(Parent, Val);
            return Val;
        }

        public FieldType Value(ParentType Parent)
        {
            return GetValue(Parent);
        }

        private static RefReturn _GetValueRef(FieldInfo Field)
        {
            var asmName = new AssemblyName("MN_DynamicAssembly." + Guid.NewGuid().ToString());
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule("<Module>");
            var typeBuilder = moduleBuilder.DefineType("Helper");
            var methodBuilder = typeBuilder.DefineMethod("GetValue_",
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(FieldType).MakeByRefType(), new Type[] { typeof(ParentType).MakeByRefType() });
            var ilGen = methodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            if (typeof(ParentType).IsClass)
                ilGen.Emit(OpCodes.Ldind_Ref);
            ilGen.Emit(OpCodes.Ldflda, Field);
            ilGen.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateType();
            var mi = type.GetMethod(methodBuilder.Name);
            var del = (RefReturn)mi.CreateDelegate(typeof(RefReturn));
            return del;
        }

        private static Func<ParentType, FieldType> _GetValue(FieldInfo Field)
        {
            var methodName = "GetValue_";
            var dynMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[] { typeof(ParentType) },restrictedSkipVisibility: true);
            ILGenerator il = dynMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, Field);
            il.Emit(OpCodes.Ret);
            return (Func<ParentType, FieldType>)dynMethod.CreateDelegate(typeof(Func<ParentType, FieldType>));
        }

        public override string ToString()
        {
            var Str = typeof(ParentType).Name;
            if(typeof(ParentType).IsGenericType)
            {
                Str += "[";
                foreach (var Arg in typeof(ParentType).GenericTypeArguments)
                {
                    Str += Arg.Name + ",";
                }
                Str += "]";
            }

            Str +=" >> " + typeof(FieldType).Name;
            if (typeof(FieldType).IsGenericType)
            {
                Str += "[";
                foreach (var Arg in typeof(FieldType).GenericTypeArguments)
                {
                    Str += Arg.Name + ",";
                }
                Str += "]";
            }
            return Str;
        }
    }

}
