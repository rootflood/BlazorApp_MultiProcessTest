using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Database.Base
{
    public partial class Table<ValueType, KeyType>
    {
        public class KeyInfo
        {
            public Monsajem_Incs.Array.Hyper.SortedArray<KeyType> Keys;
        }
    }
}
