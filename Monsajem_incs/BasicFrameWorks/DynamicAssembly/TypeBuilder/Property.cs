using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Monsajem_Incs.DynamicAssembly
{
    public partial class TypeBuilder
    {
        public PropertyBuilder Property(
            string Name,
            Type Ouput,
            PropertyAttributes attributes,
            MethodBuilder GetMethod,
            MethodBuilder SetMethod)
        {
            
            var PropBldr = myTypeBuilder.DefineProperty(Name,attributes,Ouput,null);
            PropBldr.SetGetMethod(GetMethod);
            PropBldr.SetSetMethod(SetMethod);
            return PropBldr;
        }

        public PropertyBuilder Property(
            string Name,
            Type Ouput,
            PropertyAttributes attributes,
            (MethodAttributes Attributes,
             string Name,
             Action<ILGenerator> ILGenerator)GetMethod,
            (MethodAttributes Attributes,
             string Name,
             Action<ILGenerator> ILGenerator) SetMethod)
        {
            var GetPropMthdBldr = Method(GetMethod.Attributes,GetMethod.Name,
                                         null,Ouput,
                                         GetMethod.ILGenerator);
            var SetPropMthdBldr = Method(SetMethod.Attributes,SetMethod.Name,
                                         new Type[] { Ouput },null,
                                         SetMethod.ILGenerator);
            return Property(Name,Ouput,attributes, GetPropMthdBldr, SetPropMthdBldr);
        }

        public PropertyBuilder Property(
            string Name,
            Type Ouput,
            PropertyAttributes attributes,
            (MethodAttributes Attributes,
             string Name) GetMethod,
            (MethodAttributes Attributes,
             string Name) SetMethod)
        {
            var FieldBldr = myTypeBuilder.DefineField(Name,Ouput,FieldAttributes.Private);
            return Property(Name, Ouput, attributes,
                (GetMethod.Attributes,GetMethod.Name,
                (GetIL) =>
                {
                    GetIL.Emit(OpCodes.Ldarg_0);
                    GetIL.Emit(OpCodes.Ldfld, FieldBldr);
                    GetIL.Emit(OpCodes.Ret);
                }),
                (SetMethod.Attributes,SetMethod.Name,
                (SetIL) =>
                {
                    SetIL.Emit(OpCodes.Ldarg_0);
                    SetIL.Emit(OpCodes.Ldarg_1);
                    SetIL.Emit(OpCodes.Stfld, FieldBldr);
                    SetIL.Emit(OpCodes.Ret);
                }
            ));
        }


        public PropertyBuilder Property<t>(
            string Name,
            PropertyAttributes attributes,
            MethodBuilder GetMethod,
            MethodBuilder SetMethod) =>
            Property(Name, typeof(t), attributes, GetMethod, SetMethod);

        public PropertyBuilder Property<t>(
            string Name,
            PropertyAttributes attributes,
            (MethodAttributes Attributes,
             string Name,
             Action<ILGenerator> ILGenerator) GetMethod,
            (MethodAttributes Attributes,
             string Name,
             Action<ILGenerator> ILGenerator) SetMethod) =>
            Property(Name, typeof(t), attributes, GetMethod, SetMethod);

        public PropertyBuilder Property<t>(
            string Name,
            PropertyAttributes attributes,
            (MethodAttributes Attributes,
             string Name) GetMethod,
            (MethodAttributes Attributes,
             string Name) SetMethod) =>
            Property(Name, typeof(t), attributes, GetMethod, SetMethod);

    }
}
