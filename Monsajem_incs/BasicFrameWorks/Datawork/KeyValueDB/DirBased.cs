using System;
using System.IO;
using System.Linq.Expressions;
using Monsajem_Incs.Serialization;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.Array.Base;
using static System.Text.Encoding;
using System.Threading.Tasks;
using System.Collections;

namespace Monsajem_Incs.Database.KeyValue.DirBased
{
    public class FileDictionary<KeyType, ValueType> : Base.IDictionary<KeyType, ValueType>
    {
        private string Dir;
        public FileDictionary(string Dir)
        {
            this.Dir = Dir+"\\";
            System.IO.Directory.CreateDirectory(Dir);
        }

        public bool ContainKey(KeyType Key)
        {
            return File.Exists(Dir + System.Convert.ToBase64String(Key.Serialize()));
        }

        public void DeleteItem(KeyType Key)
        {
            File.Delete(Dir + System.Convert.ToBase64String(Key.Serialize()));
        }

        public ValueType GetItem(KeyType Key)
        {
            return File.ReadAllBytes(Dir + System.Convert.ToBase64String(Key.Serialize())).Deserialize<ValueType>();
        }

        public void SetItem(KeyType Key, ValueType Value)
        {
            File.WriteAllBytes(Dir + System.Convert.ToBase64String(Key.Serialize()),Value.Serialize());
        }
    }

    public class Table<ValueType, KeyType> :
        Base.Table<ValueType, KeyType>
        where KeyType : IComparable<KeyType>
    {
        public Table(string Dir,
            Func<ValueType, KeyType> GetKey, bool IsUpdatAble) :
            base((b)=>File.WriteAllBytes(Dir+"\\K",b),
                 ()=>File.Exists(Dir + "\\K") ? File.ReadAllBytes(Dir + "\\K"):null,
                 new FileDictionary<KeyType,ValueType>(Dir+"\\V"), GetKey, IsUpdatAble)
        {
            this.TableName = new DirectoryInfo(Dir).Name;
        }
    }
}
