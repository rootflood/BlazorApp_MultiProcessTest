using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Monsajem_Incs.DynamicAssembly
{
    public partial class TypeBuilder
    {
        public MethodBuilder Method(
            MethodAttributes Attributes,
            string Name,
            Type[] Inputs,
            Type Ouput,
            Action<ILGenerator> ILGenerator)
        {
            if (Inputs == null)
                Inputs = Type.EmptyTypes;
            MethodBuilder MethodBuilder =
                    myTypeBuilder.DefineMethod(Name,
                                               Attributes,
                                               Ouput,
                                               Inputs);

            ILGenerator IL = MethodBuilder.GetILGenerator();

            ILGenerator(IL);
            return MethodBuilder;
        }

        public MethodBuilder Method(
            MethodAttributes Attributes,
            string Name,
            MethodInfo LikeMethod,
            Action<ILGenerator> ILGenerator) =>
            Method(Attributes, Name,
                   LikeMethod.GetParameters().Select((c) => c.ParameterType).ToArray(),
                   LikeMethod.ReturnType,
                   ILGenerator);

        public MethodBuilder Method<t>(
            MethodAttributes Attributes,
            string Name,
            Action<ILGenerator> ILGenerator)
            where t : MulticastDelegate =>
            Method(Attributes, Name, typeof(t).GetMethod("Invoke"), ILGenerator);

    }
}
