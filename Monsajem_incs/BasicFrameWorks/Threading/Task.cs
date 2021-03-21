using System;
using System.Linq;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
namespace Monsajem_Incs.Threading
{
    public static class Task_EX
    {
        public static async Task CheckAll(Task Task1,Task Task2)
        {
            var Checks = new Task[] { Task1, Task2 };
            var Result = await Task.WhenAny(Checks);
            if(Result.Id==Task1.Id)
            {
                await Task1;
                await Task2;
            }
            else
            {
                await Task2;
                await Task1;
            }    
        }
        public static async Task CheckAll(params Task[] Tasks)
        {
            while(Tasks.Length>0)
            {
                var Result = await Task.WhenAny(Tasks);
                await Result;
                Tasks = Tasks.Where((c) => c.Id != Result.Id).ToArray();
            }
        }
        public static async Task<Task> CheckAny(params Task[] Tasks)
        {
            var Result = await Task.WhenAny(Tasks);
            await Result;
            return Result;
        }
        public static async Task TimeOut(this Task Task,int TimeOut)
        {
            var Timer = Task.Delay(TimeOut);
            var Result = await Task.WhenAny(Task,Timer);
            if (Result.Id == Timer.Id)
                throw new Exception("Task Time Out!");
        }
        public static async Task<t> TimeOut<t>(this Task<t> task, int TimeOut)
        {
            var Timer = Task.Delay(TimeOut);
            var Result = await Task.WhenAny(task, Timer);
            if (Result.Id == Timer.Id)
                throw new Exception("Task Time Out!");
            return task.Result;
        }
    }
}