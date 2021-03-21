using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using Monsajem_Incs.Serialization;
using System.Threading.Tasks;

namespace Monsajem_Incs.Database.Base
{
    public partial class UpdateAble<KeyType>
        where KeyType : IComparable<KeyType>
    {
        public ulong UpdateCode;
        public KeyType Key;

        public static IComparer<UpdateAble<KeyType>> CompareKey = new _CompareKey();
        private class _CompareKey : IComparer<UpdateAble<KeyType>>
        {
            public int Compare(UpdateAble<KeyType> x, UpdateAble<KeyType> y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }

        public static IComparer<UpdateAble<KeyType>> CompareCode = new _CompareCode();
        private class _CompareCode : IComparer<UpdateAble<KeyType>>
        {
            public int Compare(UpdateAble<KeyType> x, UpdateAble<KeyType> y)
            {
                return x.UpdateCode.CompareTo(y.UpdateCode);
            }
        }
    }
    public partial class UpdateAbles<KeyType>
        where KeyType : IComparable<KeyType>
    {
        [ThreadStaticAttribute]
        public static DynamicAssembly.RunOnceInBlock[] IgnoreUpdateAble;
        public static int IgnoreUpdateAble_Len;

        public UpdateAbles(int Len=0)
        {
            UpdateCodes = new UpdateAble<KeyType>[Len];
            UpdateKeys = new UpdateAble<KeyType>[Len];
        }

        public UpdateAble<KeyType> this[KeyType Key] {
            get
            {
                var Place = System.Array.BinarySearch(UpdateKeys,
                    new UpdateAble<KeyType>() { Key = Key },
                    UpdateAble<KeyType>.CompareKey);
                if (Place < 0)
                    return null;
                return UpdateKeys[Place];
            }
        }

        public UpdateAble<KeyType> this[ulong Code]
        {
            get
            {
                var Place = System.Array.BinarySearch(UpdateCodes,
                    new UpdateAble<KeyType>() { UpdateCode = Code },
                    UpdateAble<KeyType>.CompareCode);
                if (Place < 0)
                    return null;
                return UpdateCodes[Place];
            }
        }

        public bool IsExist(ulong Code)
        {
            var Place = System.Array.BinarySearch(UpdateKeys,
                new UpdateAble<KeyType>() { UpdateCode = Code },
                    UpdateAble<KeyType>.CompareCode);
            return Place >= 0;
        }

        public void Update()
        {
            UpdateCode += 1;
        }

        public void Insert(KeyType Key)
        {
            UpdateCode += 1;
            Insert(Key, UpdateCode);
        }

        public void Insert(KeyType Key, ulong UpdateCode)
        {
            var Update = new UpdateAble<KeyType>() { Key = Key, UpdateCode = UpdateCode };
            var Place = System.Array.BinarySearch(UpdateKeys, Update, UpdateAble<KeyType>.CompareKey);
            if (Place >= 0)
            {
                _Changed(Key, Key, UpdateCode, Place);
                return;
            }
            _Insert(Key, UpdateCode);
        }
        private void _Insert(KeyType Key, ulong UpdateCode)
        {
            var Update = new UpdateAble<KeyType>() { Key = Key, UpdateCode = UpdateCode };
            ArrayExtentions.ArrayExtentions.BinaryInsert(
                ref UpdateCodes, Update, UpdateAble<KeyType>.CompareCode);
            ArrayExtentions.ArrayExtentions.BinaryInsert(
                ref UpdateKeys, Update, UpdateAble<KeyType>.CompareKey);
        }

        public void Delete(KeyType Key)
        {
            UpdateCode += 1;
            DeleteDontUpdate(Key);
        }
        public void DeleteDontUpdate(KeyType Key)
        {
            var Place = System.Array.BinarySearch(UpdateKeys,
                new UpdateAble<KeyType>() { Key = Key },
                UpdateAble<KeyType>.CompareKey);
            if (Place < 0)
                return;
            var Update = UpdateKeys[Place];
            ArrayExtentions.ArrayExtentions.BinaryDelete(
                ref UpdateCodes, Update, UpdateAble<KeyType>.CompareCode);
            ArrayExtentions.ArrayExtentions.DeleteByPosition(
                ref UpdateKeys, Place);
        }

        public void Changed(KeyType Old, KeyType New)
        {
            UpdateCode += 1;
            Changed(Old, New, UpdateCode);
        }

        public void Changed(KeyType Old, KeyType New,ulong UpdateCode)
        {
            var OldPlace = System.Array.BinarySearch(UpdateKeys,
                        new UpdateAble<KeyType>() { Key = Old },
                        UpdateAble<KeyType>.CompareKey);
            if(OldPlace<0)
            {
                _Insert(New, UpdateCode);
                return;
            }
            _Changed(Old, New, UpdateCode, OldPlace);
        }
        private void _Changed(KeyType Old, KeyType New, ulong UpdateCode,int OldPlace_key)
        {
            var OldUpdate = UpdateKeys[OldPlace_key];
            var NewUpdate = new UpdateAble<KeyType>() { Key = New, UpdateCode = UpdateCode };

            ArrayExtentions.ArrayExtentions.BinaryUpdate(
                UpdateKeys, OldUpdate, NewUpdate, UpdateAble<KeyType>.CompareKey);
            ArrayExtentions.ArrayExtentions.BinaryUpdate(
                UpdateCodes, OldUpdate, NewUpdate, UpdateAble<KeyType>.CompareCode);
        }

        public ulong UpdateCode;
        public UpdateAble<KeyType>[] UpdateCodes;
        public UpdateAble<KeyType>[] UpdateKeys;
    }

    public partial class Table<ValueType, KeyType>
    {
        [Serialization.NonSerialized]
        public Action<UpdateAbles<KeyType>> UpdateAbleChanged;


        [Serialization.NonSerialized]
        internal int IgnoreUpdateAble_pos;

        [Serialization.NonSerialized]
        public DynamicAssembly.RunOnceInBlock IgnoreUpdateAble
        {
            get
            {
                if (UpdateAbles<KeyType>.IgnoreUpdateAble == null)
                    UpdateAbles<KeyType>.IgnoreUpdateAble =
                        new DynamicAssembly.RunOnceInBlock[UpdateAbles<KeyType>.IgnoreUpdateAble_Len];
                else if (UpdateAbles<KeyType>.IgnoreUpdateAble.Length < UpdateAbles<KeyType>.IgnoreUpdateAble_Len)
                    System.Array.Resize(ref UpdateAbles<KeyType>.IgnoreUpdateAble,
                        UpdateAbles<KeyType>.IgnoreUpdateAble_Len);

                var MY_UpdateAble =
                     UpdateAbles<KeyType>.IgnoreUpdateAble[IgnoreUpdateAble_pos];
                if (MY_UpdateAble == null)
                {
                    MY_UpdateAble = new DynamicAssembly.RunOnceInBlock();
                    UpdateAbles<KeyType>.IgnoreUpdateAble[IgnoreUpdateAble_pos] = MY_UpdateAble;
                }
                return MY_UpdateAble;
            }
        }

        [Serialization.NonSerialized]
        private Action<KeyType> UpdateAbleChanged_InRelation;

        internal UpdateAbles<KeyType> _UpdateAble;
        public UpdateAbles<KeyType> UpdateAble
        {
            get => _UpdateAble;
            set
            {
                _UpdateAble = value;
                UpdateAbleChanged?.Invoke(_UpdateAble);
            }
        }

        protected void ReadyForUpdateAble()
        {
            UpdateAbleChanged += (_UpdateAble) =>
            {
                if (_UpdateAble != null)
                {
                    Events.Inserted += (info) =>
                    {
                        if (IgnoreUpdateAble.BlockLengths == 0)
                        {
                            _UpdateAble.Insert((KeyType)info.Info[KeyPos].Key);
                        }
                    };

                    Events.Deleted += (info) =>
                    {
                        if (IgnoreUpdateAble.BlockLengths == 0)
                        {
                            _UpdateAble.Delete((KeyType)info.Info[KeyPos].Key);
                        }
                    };

                    Events.Updated += (info) =>
                    {
                        if (IgnoreUpdateAble.BlockLengths == 0)
                        {
                            var OldPos =(KeyType) info.Info[KeyPos].OldKey;
                            var NewPos = (KeyType)info.Info[KeyPos].Key;
                            _UpdateAble.Changed(OldPos, NewPos);
                        }
                    };
                }
            };
        }
    }
}