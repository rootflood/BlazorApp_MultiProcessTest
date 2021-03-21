using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Calculator
{
    public class DirectCalculator<Input,Result>
        where Result:unmanaged
        where Input: unmanaged
    {
        public DirectCalculator(Result[] Results)
        {
            this.Results = Results;
            InputSize_inMemory = System.Runtime.InteropServices.Marshal.SizeOf<Input>();
            ResultSize_inMemory = System.Runtime.InteropServices.Marshal.SizeOf<Result>();
            BufferX = Results.Length / (ResultSize_inMemory * 256);
            ResultSize_inMemory = BufferX * ResultSize_inMemory;
        }

        private Result[] Results;
        private int BufferX;
        private int InputSize_inMemory;
        private int ResultSize_inMemory;
        public unsafe Result[] Calc(Input[] Input)
        {
            var Count = Input.Length * BufferX;
            var Result = new Result[Count];
            int ResultIndex = 0;
            int InputIndex = 0;
            fixed (Result* RefResult = Result)
            fixed (Input* RefBuffer = Input)
            fixed (Result* RefAllResults = Results)
                while (ResultIndex < Count)
                {
                    var Index = CalcPos(*(RefBuffer + InputIndex));
                    for(int X=0;X<BufferX;X++)
                    {
                        *(RefResult + ResultIndex + X) = *(RefAllResults + Index+X);
                    }
                    ResultIndex += ResultSize_inMemory;
                    InputIndex += InputSize_inMemory;
                }
            return Result;
        }
       
        public Func<Input, int> CalcPos;
    }

}
