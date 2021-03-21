using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Monsajem_Incs.Array.Hyper;
using static Monsajem_Incs.Database.Base.Runer;
using static System.Runtime.Serialization.FormatterServices;

namespace Monsajem_Incs.Database.Base
{
    public partial class Events<ValueType>
    {
        public class ValueInfo
        {
            public int Pos;
            public object Key;
        }

        public Action<ValueType> Saving;
        public Action<(ValueType Value, ValueInfo[] Info)> Inserting;
        public Action<(ValueType Value, ValueInfo[] Info)> Inserted;
    }
    public partial class SecurityEvents<ValueType>
    {
        public Action<(ValueType Value, Events<ValueType>.ValueInfo[] Info)> Inserting;
    }

    public partial class Table<ValueType, KeyType>
    {
        public int Insert(ValueType Value)
        {
            lock (this)
            {
                using (Run.Block())
                {
                    AutoFillRelations?.Invoke(Value);
                    var KeysLen = BasicActions.Keys;
                    var Info = (Value, new Events<ValueType>.ValueInfo[KeysLen]);
                    for (int i = 0; i < KeysLen; i++)
                        Info.Item2[i] = new Events<ValueType>.ValueInfo();
                    SecurityEvents.Inserting?.Invoke(Info);
                    Events.Inserting?.Invoke(Info);
                    Events.Saving?.Invoke(Value);
                    Events.Inserted?.Invoke(Info);
                    return Info.Item2[KeyPos].Pos;
                }
            }
        }

        public int Insert(Action<ValueType> ValueAction)
        {
            var Value = (ValueType)GetUninitializedObject(typeof(ValueType));
            ValueAction(Value);
            return Insert(Value);
        }

        public void Insert(IEnumerable<ValueType> Values)
        {
            foreach (var Value in Values)
                Insert(Value);
        }
    }
}
