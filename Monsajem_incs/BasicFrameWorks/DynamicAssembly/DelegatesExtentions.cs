using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monsajem_Incs.Array.Hyper;
namespace Monsajem_Incs.DynamicAssembly
{
    public static class DelegatesExtentions
    {
        public static Delegate RemoveAllDelegates(
            this Delegate Source, Delegate Removes)
        {
            var Result = Source;
            foreach (var Remove in Removes.GetInvocationList())
            {
                Result = Delegate.RemoveAll(Result, Remove);
            }
            return Result;
        }
    }

    public struct RunOnceInStack
    {
        private SortedArray<string> RunIfNotRuning_Name;
        public bool Use(string Name)
        {
            if (RunIfNotRuning_Name == null)
                RunIfNotRuning_Name = new SortedArray<string>();
            var Pos =RunIfNotRuning_Name.BinarySearch(Name).Index;
            if (Pos < 0)
            {
                Pos = ((Pos * -1) - 1);
                RunIfNotRuning_Name.Insert(Name, Pos);
                return true;
            }
            else
                return false;
        }

        public void EndUse(string Name)
        {
            RunIfNotRuning_Name.BinaryDelete(Name);
        }

        private bool RunIfAnyNotRuning_flag;
        public bool AnyUse()
        {
            if (RunIfNotRuning_Name == null)
                RunIfNotRuning_Name =new SortedArray<string>();
            if (RunIfNotRuning_Name.Length == 0 &&
                RunIfAnyNotRuning_flag == false)
            {
                RunIfAnyNotRuning_flag = true;
                return true;
            }
            else
                return false;
        }

        public void EndAnyUse()
        {
            RunIfAnyNotRuning_flag = true;
        }
    }

    public class RunOnceInBlock:IDisposable 
    {
        public event Action OnEndBlocks;
        private SortedArray<string> Used;
        private int Blocks;
        public int BlockLengths
        {
            get => Blocks;
        }
        public bool Use(string Name)
        {
            if (Used == null)
                Used = new SortedArray<string>();
            var Pos = Used.BinarySearch(Name).Index;
            if (Pos < 0)
            {
                Pos = ((Pos * -1) - 1);
                Used.Insert(Name, Pos);
                return true;
            }
            else
                return false;
        }

        public void EndUse(string Name)
        {
            Used.BinaryDelete(Name);
        }

        public RunOnceInBlock Block()
        {
            Blocks++;
            return this;
        }

        public void EndBlock()
        {
            Blocks--;
            if (Blocks == 0)
            {
                Used = new SortedArray<string>();
                OnEndBlocks?.Invoke();
            }
        }

        public void Dispose()
        {
            Blocks--;
            if (Blocks == 0)
            {
                Used = new SortedArray<string>();
                OnEndBlocks?.Invoke();
            };
        }
    }
}
