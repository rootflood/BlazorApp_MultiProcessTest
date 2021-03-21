using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Hyper
{
    public static class ArrayMaker
    {
        public static Array<ArrayType> Make<ArrayType>(ArrayType ItemType) =>
            new Array<ArrayType>();
    }
    public class Array<ArrayType> :
        BaseArray<ArrayType, Array<ArrayType>>
    {
        public Array(int MinCount = 500):
            base(MinCount)
        {}
        public Array(ArrayType[] ar, int MinCount = 500) : 
            base(ar,MinCount)
        {}

        protected override Array<ArrayType> MakeSameNew()
        {
            return new Array<ArrayType>(MinCount);
        }
    }
}