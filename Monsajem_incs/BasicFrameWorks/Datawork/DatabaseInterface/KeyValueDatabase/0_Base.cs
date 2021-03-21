using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Monsajem_Incs.Array.Hyper;
using Monsajem_Incs.Serialization;
using static System.Runtime.Serialization.FormatterServices;
using Monsajem_Incs.Net.Base.Service;
using System.Threading;

namespace Monsajem_Incs.Database.Base
{
    public interface IFactualyData
    {
        object Parent { get; set; }
    }

    public partial class BasicActions<ValueType>
    {
        public Array.Base.IArray<ValueType> Items;
    }

    public class Runer
    {
        [ThreadStatic]
        private static Monsajem_Incs.DynamicAssembly.RunOnceInBlock _Run;

        public static Monsajem_Incs.DynamicAssembly.RunOnceInBlock Run
        {
            get
            {
                if (_Run == null)
                    _Run = new Monsajem_Incs.DynamicAssembly.RunOnceInBlock();
                return _Run;
            }

        }
    }

    public partial class Table<ValueType, KeyType>
        where KeyType : IComparable<KeyType>
    {

        [RemoteExactDelegate]
        [Serialization.NonSerialized]
        public Func<ValueType, KeyType> GetKey;
        public KeyInfo KeysInfo = new KeyInfo();

        [Serialization.NonSerialized]
        public BasicActions<ValueType> BasicActions;
        [Serialization.NonSerialized]
        public Events<ValueType> Events;
        [Serialization.NonSerialized]
        public Action<ValueType> AutoFillRelations;
        [Serialization.NonSerialized]
        public SecurityEvents<ValueType> SecurityEvents;
        [Serialization.NonSerialized]
        internal string TableName;

        protected int KeyPos;

        internal Table(BasicActions<ValueType> BasicActions,
                       Func<ValueType, KeyType> GetKey,
                       KeyType[] XNewKeys,
                       int KeyPos,
                       bool IsUnique,
                       bool IsUpdateAble)
        {
            this.GetKey = GetKey;
            this.BasicActions = BasicActions;
            this.KeyPos = KeyPos;

            this.KeysInfo.Keys = new SortedArray<KeyType>(XNewKeys);

            this.SecurityEvents = new SecurityEvents<ValueType>();
            this.Events = new Events<ValueType>();

            SecurityEvents.MakeKeys += (uf) =>
            {
                uf.Info[KeyPos].OldKey = this.GetKey(uf.Value);
            };

            if (IsUnique)
            {
                SecurityEvents.Updating += (uf) =>
                {
                    var MyInfo = uf.Info[KeyPos];
                    var MyValue = uf.Value;
                    var OldKey = (KeyType)MyInfo.OldKey;
                    var NewKey = this.GetKey(MyValue);
                    var OldPos = KeysInfo.Keys.BinarySearch(OldKey).Index;
                    var NewPos = 0;
                    if (OldKey.CompareTo(NewKey) != 0)
                    {
                        NewPos = KeysInfo.Keys.BinarySearch(NewKey).Index;
                        if (NewPos > -1)
                            throw new InvalidOperationException("Value be exist!");
                        NewPos = NewPos * -1;
                        NewPos -= 1;
                        KeyChanging?.Invoke(new KeyChangeInfo()
                        {
                            NewKey = NewKey,
                            OldKey = OldKey,
                            Value = MyValue
                        });
                    }
                    else
                        NewPos = OldPos;
                    MyInfo.OldPos = OldPos;
                    MyInfo.Key = NewKey;
                    MyInfo.Pos = NewPos;
                };
            }
            else
            {
                SecurityEvents.Updating += (uf) =>
                {
                    var MyInfo = uf.Info[KeyPos];
                    var MyValue = uf.Value;
                    var OldKey = (KeyType)MyInfo.OldKey;
                    var NewKey = this.GetKey(MyValue);
                    var OldPos = KeysInfo.Keys.BinarySearch(OldKey).Index;
                    var NewPos = 0;
                    if (OldKey.CompareTo(NewKey) != 0)
                    {
                        NewPos = KeysInfo.Keys.BinarySearch(NewKey).Index;
                        if (NewPos < 0)
                        {
                            NewPos = NewPos * -1;
                            NewPos -= 1;
                        }
                        KeyChanging?.Invoke(new KeyChangeInfo()
                        {
                            NewKey = NewKey,
                            OldKey = OldKey,
                            Value = MyValue
                        });
                    }
                    else
                        NewPos = OldPos;
                    MyInfo.OldKey = OldKey;
                    MyInfo.OldPos = OldPos;
                    MyInfo.Key = NewKey;
                    MyInfo.Pos = NewPos;
                };
            }

            if (KeyPos == 0)
            {
                Events.Updated += (uf) =>
                {
                    var MyInfo = uf.Info[KeyPos];
                    var MyValue = uf.Value;
                    var OldKey = (KeyType)MyInfo.OldKey;
                    var NewKey = (KeyType)MyInfo.Key;
                    var OldPos = MyInfo.OldPos;
                    var NewPos = MyInfo.Pos;
                    if (OldPos < NewPos)
                    {
                        NewPos -= 1;
                    }

                    if(OldPos!=NewPos)
                    {
                        this.BasicActions.Items.DeleteByPosition(OldPos);
                        this.BasicActions.Items.Insert(MyValue, NewPos);
                    }
                    else
                    {
                        this.BasicActions.Items[OldPos]= MyValue;
                    }

                    if (OldKey.CompareTo(NewKey) != 0)
                    {
                        KeysInfo.Keys.DeleteByPosition(OldPos);
                        KeysInfo.Keys.Insert(NewKey, NewPos);
                        KeyChanged?.Invoke(new KeyChangeInfo()
                        {
                            NewKey = NewKey,
                            OldKey = OldKey,
                            Value = MyValue
                        });
                    }
                };
            }
            else
            {
                Events.Updated += (uf) =>
                {
                    var MyInfo = uf.Info[KeyPos];
                    var OldKey = (KeyType)MyInfo.OldKey;
                    var NewKey = (KeyType)MyInfo.Key;
                    var OldPos = MyInfo.OldPos;
                    var NewPos = MyInfo.Pos;
                    if (OldPos < NewPos)
                    {
                        NewPos -= 1;
                    }
                    if (OldKey.CompareTo(NewKey) != 0)
                    {
                        KeysInfo.Keys.DeleteByPosition(OldPos);
                        KeysInfo.Keys.Insert(NewKey, NewPos);
                        KeyChanging?.Invoke(new KeyChangeInfo()
                        {
                            NewKey = NewKey,
                            OldKey = OldKey,
                            Value = uf.Value
                        });
                    }
                };
            }

            if (IsUnique)
            {
                SecurityEvents.Inserting += (info) =>
                {
                    var NewKey = this.GetKey(info.Value);
                    var NewPos = KeysInfo.Keys.BinarySearch(NewKey).Index;
                    if (NewPos > -1)
                        throw new InvalidOperationException("Value be exist!");
                    NewPos = NewPos * -1;
                    NewPos -= 1;
                    var MyInfo = info.Info[KeyPos];
                    MyInfo.Key = NewKey;
                    MyInfo.Pos = NewPos;
                };
            }
            else
            {
                SecurityEvents.Inserting += (info) =>
                {
                    var NewKey = this.GetKey(info.Value);
                    var NewPos = KeysInfo.Keys.BinarySearch(NewKey).Index;
                    if (NewPos < 0)
                    {
                        NewPos = NewPos * -1;
                        NewPos -= 1;
                    }
                    var MyInfo = info.Info[KeyPos];
                    MyInfo.Key = NewKey;
                    MyInfo.Pos = NewPos;
                };
            }

            if (KeyPos == 0)
            {
                Events.Inserted += (info) =>
                {
                    var MyInfo = info.Info[KeyPos];
                    var Pos = MyInfo.Pos;
                    KeysInfo.Keys.Insert((KeyType)MyInfo.Key, Pos);
                    this.BasicActions.Items.Insert(info.Value, Pos);
                };
            }
            else
            {
                Events.Inserted += (info) =>
                {
                    var MyInfo = info.Info[KeyPos];
                    KeysInfo.Keys.Insert((KeyType)MyInfo.Key, MyInfo.Pos);
                };
            }

            SecurityEvents.Deleting += (info) =>
            {
                var MyInfo = info.Info[KeyPos];
                //if (MyInfo.Key==null)
                //{
                var OldKey = this.GetKey(info.Value);
                var OldPos = KeysInfo.Keys.BinarySearch(OldKey).Index;
                MyInfo.Key = OldKey;
                MyInfo.Pos = OldPos;
                //}
            };

            if (KeyPos == 0)
            {
                Events.Deleted += (info) =>
                {
                    var Pos = info.Info[KeyPos].Pos;
                    this.BasicActions.Items.DeleteByPosition(Pos);
                    KeysInfo.Keys.DeleteByPosition(Pos);
                };
            }
            else
            {
                Events.Deleted += (info) =>
                {
                    KeysInfo.Keys.DeleteByPosition(info.Info[KeyPos].Pos);
                };
            }



            if (typeof(ValueType).GetInterfaces().Where((c) =>
                c == typeof(IFactualyData)).Count() > 0)
            {
                this.Events.loading += (NewValue) =>
                {
                    ((IFactualyData)NewValue).Parent = this;
                };
                this.Events.Saving += (NewValue) =>
                {
                    ((IFactualyData)NewValue).Parent = null;
                };
            }

            if (IsUpdateAble)
            {
                ReadyForUpdateAble();
                this.UpdateAble = new UpdateAbles<KeyType>();
            }
            IgnoreUpdateAble_pos = UpdateAbles<KeyType>.IgnoreUpdateAble_Len;
            UpdateAbles<KeyType>.IgnoreUpdateAble_Len++;

        }

        internal Table() { }

        public Table(
            Array.Base.IArray<ValueType> Items,
            Func<ValueType, KeyType> GetKey,
            bool IsUpdateAble) :
            this(new BasicActions<ValueType>()
            {
                Items=Items
            }, GetKey, new KeyType[0], 0, true, IsUpdateAble)
        { }


        public override string ToString()
        {
            return "Table "+ typeof(ValueType).ToString();
        }

    }
}
