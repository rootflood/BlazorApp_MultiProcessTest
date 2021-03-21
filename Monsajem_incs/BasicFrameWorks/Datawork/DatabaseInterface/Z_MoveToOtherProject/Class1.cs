using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.MoveToOtherProject
{
    internal class Delegate
    {
        public System.Delegate _delegate;

        public static Delegate operator +(System.Delegate a, Delegate b)
        {
            if (b != null)
                return new Delegate() { _delegate = System.Delegate.Combine(a, b._delegate) };
            else
                return new Delegate() { _delegate = a };
        }

        public static Delegate operator +(Delegate a, System.Delegate b)
        {
            if (a != null)
                return new Delegate() { _delegate = System.Delegate.Combine(a._delegate, b) };
            else
                return new Delegate() { _delegate = b };
        }

        public static implicit operator Delegate(System.Delegate a)
        {
            return new Delegate() { _delegate = a };
        }
    }
}
