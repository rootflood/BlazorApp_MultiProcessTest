using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Net.Base
{
    public class Enumerable<t> : IEnumerable<t>
    {
        public int Count
        {
            set
            {
                var C = 0;
                While = () => C < value;
                MoveNext = () => value += 1;
            }
        }
        public Func<bool> While;
        public Action MoveNext;
        public Func<t> Current;
        public Action Reset;
        public Action Dispose;
        public IEnumerator GetEnumerator()
        {
            Reset?.Invoke();
            return new Enumerator(MoveNext, While, Current, Reset, Dispose);
        }

        IEnumerator<t> IEnumerable<t>.GetEnumerator()
        {
            Reset?.Invoke();
            return new Enumerator(MoveNext, While, Current, Reset, Dispose);
        }

        private class Enumerator : IEnumerator<t>
        {
            public Enumerator(
                Action MoveNext,
                Func<bool> IsEnd,
                Func<t> GetCurrent,
                Action Reset,
                Action Dispose)
            {
                P_GetCurrent = GetCurrent;
                P_MoveNext = () => P_MoveNext = MoveNext;
                P_Reset = Reset;
                P_Dispose = Dispose;
                P_IsEnd = IsEnd;
            }

            private Func<bool> P_IsEnd;
            private Action P_MoveNext;
            private Func<t> P_GetCurrent;
            private Action P_Reset;
            private Action P_Dispose;

            public t Current => P_GetCurrent();

            object IEnumerator.Current => P_GetCurrent();

            public void Dispose()
            {
                P_Dispose?.Invoke();
            }

            public bool MoveNext()
            {
                P_MoveNext?.Invoke();
                if (P_IsEnd == null)
                    return true;
                else
                    return P_IsEnd();
            }

            public void Reset()
            {
                P_Reset?.Invoke();
            }
        }
    }

}
