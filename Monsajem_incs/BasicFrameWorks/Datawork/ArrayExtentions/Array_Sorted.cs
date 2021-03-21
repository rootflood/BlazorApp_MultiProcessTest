using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monsajem_Incs.Array.Base
{
    public abstract class SortedArray<ArrayType, OwnerType>:
        IArray<ArrayType, OwnerType>
        where OwnerType : IArray<ArrayType, OwnerType>
        where ArrayType : IComparable<ArrayType>
    {
        public (int Index, ArrayType Value) BinaryDelete(ArrayType Value)
        {
            var Place = BinarySearch(Value,0,Length);
            var Index = Place.Index;
            if (Index >= 0)
            {
                DeleteByPosition(Index);
            }
            return Place;
        }
        public void BinaryDelete(IEnumerable<ArrayType> Values)
        {
            foreach (var Value in Values)
                BinaryDelete(Value);
        }
        public int BinaryInsert(ArrayType Value)
        {
            var Place = BinarySearch(Value,0,Length).Index;
            if (Place < 0)
                Place = (Place * -1) - 1;
            Insert(Value, Place);
            return Place;
        }
        public int BinaryInsert(ref ArrayType Value)
        {
            var Place = BinarySearch(Value, 0, Length);
            var Index = Place.Index;
            if (Index < 0)
            {
                Insert(Value, (Index * -1) - 1);
                return Index;
            }
            Value = Place.Value;
            return Index;
        }
        public void BinaryInsert(params ArrayType[] Values)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                BinaryInsert(Values[i]);
            }
        }
        public void BinaryInsert(IEnumerable<ArrayType> Values)
        {
            foreach (var Value in Values)
            {
                BinaryInsert(Value);
            }
        }
        public (int Index, ArrayType Value) BinarySearch(ArrayType key)
        {
            return BinarySearch(key, 0, Length);
        }
        public (int Index, ArrayType Value) BinarySearch(ArrayType key,int minNum,int maxNum)
        {
            maxNum = minNum + maxNum-1 ;
            int mid =0;
            ArrayType Value;
            while (minNum <= maxNum)
            {
                mid = (minNum + maxNum) / 2;
                Value = this[mid];
                var cmp = Value.CompareTo(key);
                if (cmp == 0)
                {
                    return (mid,Value);
                }
                else if (cmp > 0)
                {
                    maxNum = mid - 1;
                }
                else
                {
                    minNum = mid + 1;
                }
            }
            return ((minNum + 1) * -1,default);
        }
    }
}
