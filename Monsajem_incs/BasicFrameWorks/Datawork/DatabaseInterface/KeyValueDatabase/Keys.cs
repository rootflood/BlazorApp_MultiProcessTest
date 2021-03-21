using System.Linq.Expressions;
using System;

namespace Monsajem_Incs.Database.Base
{

    public partial class BasicActions<ValueType>
    {
        public int Keys = 1;
    }

    public partial class Table<ValueType, KeyType>
    {
        public Table<ValueType,NewKeyType> MakeKey<NewKeyType>(Func<ValueType, NewKeyType> GetKey)
            where NewKeyType : IComparable<NewKeyType>
        {
            var Result = new Table<ValueType,NewKeyType>(
                this.BasicActions,GetKey,new NewKeyType[0],BasicActions.Keys,false,false);
            BasicActions.Keys += 1;
            return Result;
        }

        public Table<ValueType,NewKeyType> MakeUniqueKey<NewKeyType>(Func<ValueType, NewKeyType> GetKey)
           where NewKeyType : IComparable<NewKeyType>
        {
            var Result = new Table<ValueType, NewKeyType>(
                this.BasicActions, GetKey, new NewKeyType[0], BasicActions.Keys, true,false);
            BasicActions.Keys += 1;
            return Result;
        }
    }
}