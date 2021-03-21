using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static Monsajem_Incs.Database.Base.Runer;
using static System.Runtime.Serialization.FormatterServices;

namespace Monsajem_Incs.Database.Base
{
    public partial class PartOfTable<ValueType, KeyType>
    {
        public partial class TableExtras
        {
            public Action<KeyInfo> Ignoring;
            public Action<KeyInfo> Ignored;
        }

        public int Ignore(KeyType Key)
        {
            using (Run.Block())
            {
                var Keyinfo = new TableExtras.KeyInfo();
                Keyinfo.Key = Key;
                Extras.Ignoring?.Invoke(Keyinfo);
                Keyinfo.Pos = KeysInfo.Keys.BinaryDelete(Key).Index;
                Extras.Ignored?.Invoke(Keyinfo);
                return Keyinfo.Pos;
            }
        }

        public void Ignore(Action<ValueType> ValueCreator)
        {
            var Value = (ValueType)GetUninitializedObject(typeof(ValueType));
            ValueCreator(Value);
            Ignore(Value);
        }

        public void Ignore(ValueType Value)
        {
            var Key =base.GetKey(Value);
            Ignore(Key);
        }

        public void Ignore(Table<ValueType, KeyType> Values)
        {
            foreach (var Key in Values.KeysInfo.Keys)
                Ignore(Key);
        }

        public void Ignore(IEnumerable<ValueType> Values)
        {
            foreach (var Value in Values)
                Ignore(Value);
        }
        public void Ignore(IEnumerable<KeyType> Keys)
        {
            foreach (var Key in Keys)
                Ignore(Key);
        }

        public void Ignore()
        {
            foreach (var Key in KeysInfo.Keys.ToArray())
                Ignore(Key);
        }
    }
}
