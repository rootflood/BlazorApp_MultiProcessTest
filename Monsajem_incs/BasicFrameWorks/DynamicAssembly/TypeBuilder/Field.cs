using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Monsajem_Incs.DynamicAssembly
{
    public partial class TypeBuilder
    {

        public FieldBuilder DeclareField(
            FieldAttributes FieldAttribute,string Name, Type Type) =>
            myTypeBuilder.DefineField(Name, Type, FieldAttribute);

        public FieldBuilder DeclareField<t>(
            FieldAttributes FieldAttribute,string Name)=>
            DeclareField(FieldAttribute,Name, typeof(t));
    }
}
