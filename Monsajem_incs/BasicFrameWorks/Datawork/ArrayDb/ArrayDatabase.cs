using System;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.Serialization;

namespace Monsajem_Incs.Database
{
    public class ArrayTable<ValueType,KeyType>:
        Table<ValueType,KeyType>
        where KeyType:IComparable<KeyType>
        
    {
        public Array.Hyper.Array<ValueType> Values= new Array.Hyper.Array<ValueType>();
        public ArrayTable(
            Func<ValueType,KeyType> GetKey,
            bool IsUpdateAble):
            base(new Array.Hyper.Array<ValueType>(), GetKey,
                 IsUpdateAble)
        {
        }
    }
}