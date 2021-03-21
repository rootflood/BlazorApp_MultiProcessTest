using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using Monsajem_Incs.Array.Hyper;
using Monsajem_Incs.Serialization;

namespace Monsajem_Incs.Database.Base
{
    public partial class PartOfTable<ValueType, KeyType> :
        Table<ValueType, KeyType>
        where KeyType : IComparable<KeyType>
    {

        [Serialization.NonSerialized]
        public TableExtras Extras;

        [Serialization.NonSerialized]
        internal Table<ValueType, KeyType> Parent;

        [Serialization.NonSerialized]
        internal Action SaveToParent;

        public PartOfTable(KeyType[] NewKEys, Table<ValueType, KeyType> Parent)
        {
            this.Parent = Parent;

            base.BasicActions = new BasicActions<ValueType>();
            base.Events = new Events<ValueType>();
            base.SecurityEvents = new SecurityEvents<ValueType>();

            base.BasicActions.Items = new Array.Base.DynamicArray<ValueType>()
            {
                _GetItem = (pos) => Parent.GetItem(KeysInfo.Keys[pos]).Value
            };
            base.BasicActions.Keys = Parent.BasicActions.Keys;

            base.Events.Inserting += (inf) => Parent.Events.Inserting?.Invoke(inf);
            base.SecurityEvents.Inserting += (inf) => Parent.SecurityEvents.Inserting?.Invoke(inf);
            base.Events.Inserted += (inf) => Parent.Events.Inserted?.Invoke(inf);

            base.SecurityEvents.Deleting += (inf) => Parent.SecurityEvents.Deleting?.Invoke(inf);
            base.Events.Deleting += (inf) => Parent.Events.Deleting?.Invoke(inf);
            base.Events.Deleted += (inf) => Parent.Events.Deleted?.Invoke(inf);

            base.SecurityEvents.MakeKeys += (inf) => Parent.SecurityEvents.MakeKeys?.Invoke(inf);
            base.SecurityEvents.Updating += (inf) => Parent.SecurityEvents.Updating?.Invoke(inf);
            base.Events.Updating += (inf) => Parent.Events.Updating?.Invoke(inf);
            base.Events.Updated += (inf) =>
            {
                var OldKey = (KeyType)inf.Info[KeyPos].OldKey;
                var NewKey = (KeyType)inf.Info[KeyPos].Key;
                if (OldKey.CompareTo(NewKey) != 0)
                {
                    this.KeysInfo.Keys.BinaryDelete(OldKey);
                    this.KeysInfo.Keys.BinaryInsert(NewKey);
                }
                Parent.Events.Updated?.Invoke(inf);
            };

            base.Events.loading = Parent.Events.loading;
            base.Events.Saving = Parent.Events.Saving;

            base.GetKey = Parent.GetKey;


            base.KeysInfo.Keys = new SortedArray<KeyType>(NewKEys);

            base.Events.Inserted += (inf) =>
            {
                Accept(GetKey(inf.Value));
            };

            base.Events.Deleted += (info) =>
            {
                Ignore(GetKey(info.Value));
            };

            this.Extras = new TableExtras();

            Extras.Accepting += (Key) =>
            {
                if (KeysInfo.Keys.BinarySearch(Key.Key).Index > -1)
                    throw new InvalidOperationException("Value be exist!");
            };

            UpdateAbleChanged += (_UpdateAble) =>
            {
                if (_UpdateAble != null)
                    ReadyForUpdateAble();
            };
        }

        internal new void ReadyForUpdateAble()
        {
            Extras.Accepting += (TableExtras.KeyInfo info) =>
            {
                     _UpdateAble.Insert(info.Key);
            };

            Extras.Ignoring += (TableExtras.KeyInfo info) =>
            {
                _UpdateAble.Delete(info.Key);
            };
        }

        public override string ToString()
        {
            return "PartTable " + typeof(ValueType).ToString();
        }
    }
}
