using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monsajem_Incs.Net.Base.Service;
using Monsajem_Incs.Database;
using System.Linq;
using Monsajem_Incs.Array.Hyper;
using System.Linq.Expressions;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {

        public static void SendUpdate<DataType, KeyType>
            (this ISyncOprations Client, Table<DataType, KeyType> Table)
            where KeyType : IComparable<KeyType>
            => ((IAsyncOprations)Client).SendUpdate(Table).Wait();

        public static bool GetUpdate<DataType, KeyType>(
            this ISyncOprations Client,
            Table<DataType, KeyType> Table,
            Action<DataType> MakeingUpdate = null)
            where KeyType : IComparable<KeyType>
            => Client.I_GetUpdate(Table, MakeingUpdate,false).GetAwaiter().GetResult();

        public static void SendUpdate<DataType, KeyType>
            (this ISyncOprations Client,
            PartOfTable<DataType, KeyType> Table)
            where KeyType : IComparable<KeyType>
            => ((IAsyncOprations)Client).SendUpdate(Table).Wait();

        public static bool GetUpdate<DataType, KeyType>(
            this ISyncOprations Client,
            PartOfTable<DataType, KeyType> RelationTable,
            Action<DataType> MakeingUpdate = null)
            where KeyType : IComparable<KeyType>
            => Client.I_GetUpdate(RelationTable, MakeingUpdate,true).GetAwaiter().GetResult();
    }
}
