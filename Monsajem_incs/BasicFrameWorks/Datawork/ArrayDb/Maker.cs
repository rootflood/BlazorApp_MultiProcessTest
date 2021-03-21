using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monsajem_Incs.Serialization;
namespace Monsajem_Incs.Database.ArrayDb
{
    public class DbMaker
    {
        public static Base.Table<t,k> Make<t,k>(Func<t,k> GetKey,bool IsUpdateAble)
            where k:IComparable<k>
        {
            var ar = new Array.Hyper.Array<t>();
            return new Monsajem_Incs.Database.Base.Table<t, k>(
                ar,GetKey, IsUpdateAble);
        }
    }
}
