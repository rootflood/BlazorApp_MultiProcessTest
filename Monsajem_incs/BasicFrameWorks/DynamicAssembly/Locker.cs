using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Monsajem_Incs.DelegateExtentions;

namespace Monsajem_Incs.DynamicAssembly
{
    public class Locked:IDisposable
    {
        internal Locked()
        { }
        internal Action Unlock;

        public static Locked operator +(Locked a, Locked b)
        {
            return new Locked() { Unlock = a.Unlock + b.Unlock };
        }

        public static Locked operator &(Locked a, Locked b)
        {
            return new Locked() { Unlock = a.Unlock + b.Unlock };
        }

        public void Dispose()
        {
            Unlock();
            System.GC.SuppressFinalize(this);
        }
    }

    public class Locker<ResourceType>
    {
        public event Action Changed;
        private ResourceType _Value;
        public ResourceType Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                Changed?.Invoke();
            }
        }
        public ResourceType LockedValue
        {
            get
            {
                using (this.Lock())
                    return _Value;
            }
            set
            {
                using (this.Lock())
                    _Value = value;
                Changed?.Invoke();
            }
        }

        public Task WaitForChange()
        {
            return Actions.WaitForHandle(() => ref Changed);
        }

        public Locked Lock()
        {
            Monitor.Enter(this);
            return new Locked()
            {
                Unlock = () => Monitor.Exit(this)
            };
        }

        public void Action(Action AC)
        {
            lock(this)
            {
                AC();
            }
        }
    }
}