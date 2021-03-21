using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
namespace Monsajem_Incs.SafeAccess
{

    public class SafeAccess<KeyType>
        where KeyType:IComparable<KeyType>
    {
        private ReaderWriterLockSlim ThisSafe = new ReaderWriterLockSlim();

        private KeyType[] Keys = new KeyType[0];
        private ReaderWriterLockSlim[] Access = new ReaderWriterLockSlim[0];

        public void Read(KeyType Key, Action Action)
        {
            var RwLock= FindRW(Key);
            RwLock.EnterReadLock();
            Action();
            RwLock.ExitReadLock();
            CheckForRelase(RwLock,Key);
        }

        public void Write(KeyType Key, Action Action)
        {
            var RwLock = FindRW(Key);
            RwLock.EnterWriteLock();
            Action();
            RwLock.ExitWriteLock();
            CheckForRelase(RwLock, Key);
        }

        private void CheckForRelase(ReaderWriterLockSlim RwLock, KeyType Key)
        {
            if (RwLock.CurrentReadCount == 0 &
                RwLock.WaitingReadCount == 0 &
                RwLock.WaitingWriteCount == 0)
            {
                ThisSafe.EnterWriteLock();
                var position = System.Array.BinarySearch(Keys, Key);
                if(position>-1)
                {
                    DeleteByPosition(ref this.Access, position);
                    DeleteByPosition(ref this.Keys, position);
                }
                ThisSafe.ExitWriteLock();
            }
        }

        private ReaderWriterLockSlim FindRW(KeyType Key)
        {
            ThisSafe.EnterWriteLock();

            int position;
            ReaderWriterLockSlim RwLock;
            position = System.Array.BinarySearch(Keys, Key);
            if (position < 0)
            {
                position = BinaryInsert(ref Keys, Key);
                RwLock = new ReaderWriterLockSlim();
                Insert(ref this.Access, RwLock, position);
                
            }
            else
            {
                RwLock = this.Access[position];
            }
            ThisSafe.ExitWriteLock();
            return RwLock;
        }
    }
}
