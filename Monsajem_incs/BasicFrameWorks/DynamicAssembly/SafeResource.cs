using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Monsajem_Incs.Array.DynamicSize;
using System.Collections.Generic;

namespace Monsajem_Incs.DynamicAssembly
{
    public class SafeResource
    {
        private Task LastTask;

        public void Safe(Task Task)
        {
            Task LastTask;
            lock (this)
            {
                LastTask = this.LastTask;
                this.LastTask = Task;
            }
            LastTask?.Wait();
            Task.Start();
            Task.Wait();
        }
    }
}
