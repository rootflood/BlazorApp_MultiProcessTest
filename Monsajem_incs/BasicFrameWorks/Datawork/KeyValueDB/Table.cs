using System;
using System.IO;
using System.Linq.Expressions;
using Monsajem_Incs.Serialization;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.Array.Base;
using static System.Text.Encoding;
using System.Threading.Tasks;
using System.Collections;

namespace Monsajem_Incs.Database.KeyValue.Base
{
    public interface IDictionary<KeyType,ValueType>
    {
        bool ContainKey(KeyType Key);
        ValueType GetItem(KeyType Key);
        void SetItem(KeyType Key,ValueType Value);
        void DeleteItem(KeyType Key);
    }

    public class Table<ValueType, KeyType> :
        Monsajem_Incs.Database.Base.Table<ValueType, KeyType>,
        IDisposable
        where KeyType : IComparable<KeyType>
    {
        [Monsajem_Incs.Serialization.NonSerialized]
        private bool NeedToSave = true;
        private Action Save;

        public Table(
            Action<byte[]> SaveKeys,
            Func<byte[]> LoadKeys,
            IDictionary<KeyType, ValueType> Data, 
            Func<ValueType, KeyType> GetKey, bool IsUpdateAble) :
            base(new DynamicArray<ValueType>(), GetKey, false)
        {
            var OldData = LoadKeys();

            if (OldData != null)
            {
                var OldTable = OldData.Deserialize(this);
                this.KeysInfo.Keys = OldTable.KeysInfo.Keys;
                if (IsUpdateAble)
                {
                    ReadyForUpdateAble();
                }
                this.UpdateAble = OldTable.UpdateAble;
            }
            else
            {
                if (IsUpdateAble)
                {
                    ReadyForUpdateAble();
                    this.UpdateAble = new UpdateAbles<KeyType>();
                }
            }

            var Ar = (DynamicArray<ValueType>)this.BasicActions.Items;
            
            Ar._GetItem = (Pos) =>
            {
                return Data.GetItem(this.KeysInfo.Keys[Pos]);
            };

            Ar._SetItem = (Pos, Value) => Data.SetItem(this.KeysInfo.Keys[Pos],Value);
            Ar._GetFromTo = (c1, c2) => default;
            Ar._SetFromTo = (c1, c2) => { };
            Ar._DeleteByPosition = (c) =>
            {
                Data.DeleteItem(this.KeysInfo.Keys[c]);
                Ar.Length -= 1;
            };
            Ar._AddLength = (count) => Ar.Length += count;
            Ar.Length = KeysInfo.Keys.Length;

            Save = () =>
            {
                SaveKeys(this.Serialize());
            };

            if (true == true) //is fast Save
            {
                ((Action)(async () =>
                {
                    save:
                    try
                    {
                        await Task.Delay(1000);
                    }
                    catch
                    {
                        goto save;
                    }
                    if (this.NeedToSave == true)
                    {
                        Save();
                        NeedToSave = false;
                    }
                    goto save;
                }))();

                this.Events.Inserted += (info) =>
                {
                    lock (this)
                    {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                    }
                };
                this.Events.Deleted += (info) =>
                {
                    lock (this)
                    {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                    }
                };
                this.KeyChanged += (info) =>
                {
                    lock (this)
                    {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                    }
                };
                this.Events.Updated += (info) =>
                {
                    lock (this)
                    {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                    }
                };
            }
            else
            {
                Runer.Run.OnEndBlocks += () => Save();
            }
        }

        [Monsajem_Incs.Serialization.NonSerialized]
        private bool IsDisposed;
        public void Dispose()
        {
            if(IsDisposed==false)
            {
                IsDisposed = true;
                Save();
                System.GC.SuppressFinalize(this);
            }
        }
    }
}