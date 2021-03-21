using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {
        public class StructrureInfo<t,r>
        {
            public StructrureInfo(
                Func<StructrureInfo<t, r>, r> Function,
                IEnumerable<t> Data)
            {
                this.Function = Function;
                this.Data = Data;
            }

            public IEnumerable<t> Data;
            private Func<StructrureInfo<t, r>, r> Function;

            public r Repliy()
            {
                return Data.ToStructrure(Function);
            }
        }

        public static r ToStructrure<t,r>(this IEnumerable<t> Table,
            Func<StructrureInfo<t,r>,r> Function)
        {
           return Function(new StructrureInfo<t, r>(Function,Table));
        }
    }
}
