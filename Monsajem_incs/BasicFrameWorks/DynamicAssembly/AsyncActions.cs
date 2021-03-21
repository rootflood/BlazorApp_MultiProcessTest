using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.DelegateExtentions
{
    public delegate ref t GetRef<t>();
    public static class Actions
    {
        public static void ex<t>(this t dg)
            where t : System.MulticastDelegate
        {

        }

        public static Task WaitForHandle(GetRef<Action> Action)
        {
            var Task = new Task(() => { });
            Action MyAction = null;
            MyAction = () => {
                Task.Start();
                Action() -= MyAction;
            };
            Action() += MyAction;
            return Task;
        }

        public static void WaitForHandle(GetRef<Action> Action,Action Handle)
        {
            var Task = new Task(() => { });
            Action MyAction = null;
            MyAction = () => {
                Handle();
                Action() -= MyAction;
            };
            Action() += MyAction;
        }

        public static Task WaitForHandle(params GetRef<Action>[] Actions)
        {
            var Task = new Task(() => { });
            Action MyAction = null;
            MyAction = () => {
                Task.Start();
                foreach(var Action in Actions)
                    Action() -= MyAction;
            };
            foreach (var Action in Actions)
                Action() += MyAction;
            return Task;
        }
        public static Task<t> WaitForHandle<t>(GetRef<Action<t>> Action)
        {
            t Result = default(t);
            var Task = new Task<t>(() => Result);
            Action<t> MyAction = null;
            MyAction = (v) => {
                Result = v;
                Task.Start();
                Action() -= MyAction;
            };
            Action() += MyAction;
            return Task;
        }

        public static Task RunAsync(Action Action)
        {
            var Task = new Task(Action);
            Task.Start();
            return Task;
        }

        public static Task RunAsync<t>(Action<t> Action,t Value)
        {
            var Task = new Task(()=>Action(Value));
            Task.Start();
            return Task;
        }

        public static t WaitForResult<t>(Task<t> task)
        {
            task.Wait();
            return task.Result;
        }

        public static void ToNewThreade(Task Task)
        {
            new System.Threading.Thread(async() =>
            {
               await Task;
            }).Start();
        }

        public static void RunOnNewThreade(Action Action)
        {
            new System.Threading.Thread(() =>
            {
                Action();
            }).Start();
        }
        public static void RunOnNewThreade(IEnumerable<Action> Actions)
        {
            foreach (var Action in Actions)
                new System.Threading.Thread(() =>Action()).Start();
        }

        public static void ToNewThreade<t>(Task<t> Task)
        {
            new System.Threading.Thread(async () =>
            {
                await Task;
            }).Start();
        }

        public static void ToNewThreade(IEnumerable<Task> Tasks)
        {
            new System.Threading.Thread(() =>
            {
                foreach (var Task in Tasks)
                {
                    new System.Threading.Thread(async () =>
                    {
                        await Task;
                    }).Start();
                }
            }).Start();
        }
    }
    public static class Extentions
    {
        public static void WaitForHandle(this GetRef<Action> Action)=>
           Actions.WaitForHandle(Action);
        public static Task<t> WaitForHandle<t>(GetRef<Action<t>> Action)=>
            Actions.WaitForHandle(Action);

        public static Task RunAsync(this Action Action)=>
            Actions.RunAsync(Action);

        public static Task RunAsync<t>(this Action<t> Action, t Value)=>
            Actions.RunAsync(Action,Value);

        public static t WaitForResult<t>(this Task<t> task) =>
            Actions.WaitForResult(task);

        public static void ToNewThreade(this Task Task)=>
            Actions.ToNewThreade(Task);

        public static void RunOnNewThreade<t>(this Action Action)=>
            Actions.RunOnNewThreade(Action);

        public static void RunOnNewThreade(this IEnumerable<Action> actions)=>
            Actions.RunOnNewThreade(actions);

        public static void ToNewThreade<t>(this Task<t> Task)=>
            Actions.ToNewThreade(Task);

        public static void ToNewThreade(this IEnumerable<Task> Tasks)=>
            Actions.ToNewThreade(Tasks);
    }
}
