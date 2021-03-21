using System;
using System.IO;
using System.Linq.Expressions;
using Monsajem_Incs.Serialization;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.Array.Base;
using static System.Text.Encoding;
using System.Threading.Tasks;
using System.Collections;

namespace Monsajem_Incs.Database.KeyValue.WebStorageBased
{
    internal class MyUTF
    {
        public static byte[] GetBytes(string str)
        {
            return System.Convert.FromBase64String(str);
            //byte[] bytes = new byte[str.Length * 2];
            //System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            //return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
            //char[] chars = new char[(bytes.Length / 2) + bytes.Length % 2];
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //return new string(chars);
        }
    }

    public static class SharedActions
    {
        public static Func<string, bool> ContainKey;
        public static Func<string, string> GetItem;
        public static Action<string, string> SetItem;
        public static Action<string> DeleteItem;
    }

    public class StorageDictionary<KeyType, ValueType> : Base.IDictionary<KeyType, ValueType>
    {
        private string StorageKey;
        public StorageDictionary(string Key)
        {
            this.StorageKey = Key;
        }

        public bool ContainKey(KeyType Key)
        {
            return SharedActions.ContainKey(StorageKey + MyUTF.GetString(Key.Serialize()));
        }

        public void DeleteItem(KeyType Key)
        {
            SharedActions.DeleteItem(StorageKey + MyUTF.GetString(Key.Serialize()));
        }

        public ValueType GetItem(KeyType Key)
        {
            var Str_Key = SharedActions.GetItem(StorageKey + MyUTF.GetString(Key.Serialize()));
            return MyUTF.GetBytes(Str_Key).Deserialize<ValueType>();
        }

        public void SetItem(KeyType Key, ValueType Value)
        {
            SharedActions.SetItem(
                  StorageKey + MyUTF.GetString(Key.Serialize()),
                  MyUTF.GetString(Value.Serialize()));
        }
    }

    public class Table<ValueType, KeyType> :
        Base.Table<ValueType, KeyType>
        where KeyType : IComparable<KeyType>
    {
        public Table(string TableName,
            Func<ValueType, KeyType> GetKey, bool IsUpdatAble) :
            base(
                 (b) =>
                 {
                     var KeyName = "K" + MyUTF.GetString(TableName.Serialize());
                     SharedActions.SetItem(KeyName, MyUTF.GetString(b));
                 },
                 () =>
                 {
                     var KeyName = "K" + MyUTF.GetString(TableName.Serialize());
                     if (SharedActions.ContainKey(KeyName))
                         return MyUTF.GetBytes(SharedActions.GetItem(KeyName));
                     return null;
                 },
                 new StorageDictionary<KeyType, ValueType>("V" + TableName), GetKey, IsUpdatAble)
        {
            this.TableName = TableName;
        }
    }
}
