using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using Monsajem_Incs.Serialization;
using static System.Runtime.Serialization.FormatterServices;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {

        public static void UpdateOrInsert<ValueType, KeyType>
            (this Table<ValueType, KeyType> Table,
            Action<ValueType> CreateOldValue,
            ValueType NewValue)
            where ValueType : new()
            where KeyType : IComparable<KeyType>
        {
            var OldValue = new ValueType();
            CreateOldValue(OldValue);
            if (Table.PositionOf(OldValue) > -1)
                Table.Update(OldValue, NewValue);
            else
                Table.Insert(NewValue);
        }

        public static void UpdateOrInsert<ValueType, KeyType>
            (this Table<ValueType, KeyType> Table,
            Action<ValueType> CreateOldValue,
            Action<ValueType> CreateNewValue)
            where ValueType : new()
            where KeyType : IComparable<KeyType>
        {
            var OldValue = new ValueType();
            CreateOldValue(OldValue);
            if (Table.PositionOf(OldValue) > -1)
                Table.Update(OldValue, CreateNewValue);
            else
            {
                Table.Insert(OldValue);
                Table.Update(OldValue, CreateNewValue);
            }
        }
    }
}

namespace Monsajem_Incs.Database.Base
{
    public partial class Table<ValueType, KeyType>
    {
        public void UpdateOrInsert(ValueType OldValue)
        {
            if (PositionOf(OldValue) > -1)
                Update(OldValue);
            else
                Insert(OldValue);
        }

        public void UpdateOrInsert(ValueType OldValue, ValueType NewValue)
        {
            if (PositionOf(OldValue) > -1)
                Update(OldValue,NewValue);
            else
                Insert(NewValue);
        }

        public void UpdateOrInsert(KeyType OldKey, ValueType NewValue)
        {
            if (PositionOf(OldKey) > -1)
                Update(OldKey, NewValue);
            else
                Insert(NewValue);
        }

        public ValueType UpdateOrInsert(KeyType OldKey, Action<ValueType> NewValueCreator)
        {
            return Update(OldKey, NewValueCreator);
        }

        public ValueType UpdateOrInsert(ValueType OldValue, Action<ValueType> NewValueCreator)
        {
            return Update(GetKey(OldValue), NewValueCreator);
        }

        public void UpdateOrInsert(Action<ValueType> NewValueCreator)
        {
            var Value = (ValueType)GetUninitializedObject(typeof(ValueType));
            NewValueCreator(Value);
            if (PositionOf(Value) > -1)
                Update(Value,Value);
            else
                Insert(Value);
        }

        public void UpdateOrInsert(Action<ValueType> NewValueCreator, Action<ValueType> Updator)
        {
            var Value = (ValueType)GetUninitializedObject(typeof(ValueType));
            NewValueCreator(Value);
            if (PositionOf(Value) < 0)
                Insert(Value);
            Update(Value, Updator);
        }
    }
}
