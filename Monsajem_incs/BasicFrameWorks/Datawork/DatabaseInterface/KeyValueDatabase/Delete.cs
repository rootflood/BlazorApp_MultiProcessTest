using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Monsajem_Incs.Array;
using static Monsajem_Incs.Database.Base.Runer;
using static System.Runtime.Serialization.FormatterServices;
namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {
        
    }
}

namespace Monsajem_Incs.Database.Base
{
    public partial class Events<ValueType>
    {
        public Action<(ValueType Value, ValueInfo[] Info)> Deleting;
        public Action<(ValueType Value, ValueInfo[] Info)> Deleted;
    }

    public partial class SecurityEvents<ValueType>
    {
        public Action<(ValueType Value, Events<ValueType>.ValueInfo[] Info)> Deleting;
    }

    public partial class Table<ValueType, KeyType>
    {

        private void IDelete(ValueType Value, KeyType Key, int Pos)
        {
            lock(this)
            {
                using (Run.Block())
                {

                    var KeysLen = BasicActions.Keys;
                    var info = (Value, new Events<ValueType>.ValueInfo[KeysLen]);
                    for (int i = 0; i < KeysLen; i++)
                        info.Item2[i] = new Events<ValueType>.ValueInfo();
                    //var MyInfo = info.Item2[KeyPos];
                    //MyInfo.Key = Key;
                    //MyInfo.Pos = Pos;
                    SecurityEvents.Deleting?.Invoke(info);
                    Events.Deleting?.Invoke(info);
                    Events.Deleted?.Invoke(info);
                }
            }
        }

        public void Delete(int Position)
        {
            var Item = GetItem(Position);
            IDelete(Item.Value, GetKey(Item.Value), Position);
        }

        public void Delete(ValueType Value)
        {
            Delete(GetKey(Value));
        }

        public void Delete(KeyType Key)
        {
            var Item = GetItem(Key);
            IDelete(Item.Value, Key, Item.Pos);
        }

        public void Delete
            (Action<ValueType> CreateOldValue)
        {
            var OldValue = (ValueType)GetUninitializedObject(typeof(ValueType));
            CreateOldValue(OldValue);
            Delete(OldValue);
        }

        public void Delete(Table<ValueType, KeyType> Values)
        {
            foreach (var Value in Values)
                Delete(GetKey(Value.Value));
        } 

        public void Delete(IEnumerable<ValueType> Values)
        {
            foreach (var Value in Values)
                Delete(GetKey(Value));
        }

        public void Delete(IEnumerable<KeyType> Keys)
        {
            foreach (var Key in Keys)
                Delete(Key);
        }

        public void Delete(Action<ValueInfo> Action = null)
        {
            if (Action == null)
            {
                foreach (var Key in KeysInfo.Keys)
                    Delete(Key);
            }
            else
            {

                foreach (var Key in this.KeysInfo.Keys)
                {
                    var Info = GetItem(Key);
                    IDelete(Info.Value, Key, Info.Pos);
                    Action(Info);
                }
            }
        }
    }
}