using System.Collections;
using System.Collections.Generic;

namespace Monsajem_Incs.Database.Base
{
    public partial class Table<ValueType, KeyType> :
        IEnumerable<Table<ValueType,KeyType>.ValueInfo>
    {
        public IEnumerator<ValueInfo> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator :
            IEnumerator<ValueInfo>
        {
            public Enumerator(Table<ValueType,KeyType> parent)
            {
                this.Parent = parent;
                this.Keys = parent.KeysInfo.Keys.ToArray();
            }

            private Table<ValueType,KeyType> Parent;
            private KeyType[] Keys;
            private int Position = -1;


            public ValueInfo Current => Parent.GetItem(Keys[Position]);

            object IEnumerator.Current => Parent.GetItem(Keys[Position]);

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                Position++;
                return Position < Keys.Length;
            }

            public void Reset()
            {
                Position = -1;
            }
        }
    }
}
