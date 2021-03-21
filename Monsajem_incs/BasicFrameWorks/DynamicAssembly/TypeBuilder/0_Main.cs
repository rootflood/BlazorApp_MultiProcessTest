using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Monsajem_Incs.DynamicAssembly
{
    public partial class TypeBuilder
    {
        private AppDomain myDomain = System.Threading.Thread.GetDomain();
        private AssemblyName myAsmName = new AssemblyName("MyDynamicAssembly");
        private AssemblyBuilder myAsmBuilder;

        private ModuleBuilder myModBuilder;
        private System.Reflection.Emit.TypeBuilder myTypeBuilder;


        public TypeBuilder(
            string TypeName,
            TypeAttributes TypeAttribute)
        {
            myAsmBuilder =
                AssemblyBuilder.DefineDynamicAssembly(myAsmName,AssemblyBuilderAccess.Run);
            myModBuilder=myAsmBuilder.DefineDynamicModule(myAsmName.Name);
            myTypeBuilder = myModBuilder.DefineType(TypeName, TypeAttribute);
        }


        public System.Reflection.Emit.TypeBuilder Builder
        {
            get => myTypeBuilder;
        }

        public Type Create()
        {
            var Result = myTypeBuilder.CreateType();
            return Result;
        }
    }
}
