using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;


namespace Monsajem_Incs.Database.Base
{

    public partial class Table<ValueType, KeyType>
    {
        public int PositionOf(ValueType Value)
        {
            AutoFillRelations?.Invoke(Value);
            return PositionOf(GetKey(Value));
        }

        public int PositionOf(KeyType Key)
        {
            lock(this)
            {
                int Result = 0;

                Result = KeysInfo.Keys.BinarySearch(Key).Index;

                return Result;
            }
        }

        public int PositionOfEqualOrAfter(KeyType Key)
        {
            lock(this)
            {
                int Pos = 0;

                Pos = KeysInfo.Keys.BinarySearch(Key).Index;
                if (Pos < 0)
                {
                    Pos = Pos * -1;
                    if (Pos > KeysInfo.Keys.Length)
                        Pos = -1;
                    else
                        Pos = Pos - 1;
                }

                return Pos;
            }
        }

        public int PositionOfAfter(KeyType Key)
        {
            lock(this)
            {
                var Pos = KeysInfo.Keys.BinarySearch(Key).Index;
                if (Pos < 0)
                {
                    Pos = Pos * -1;
                    if (Pos > KeysInfo.Keys.Length)
                        return -1;
                    return Pos;
                }
                if (Pos == KeysInfo.Keys.Length - 1)
                    return -1;
                return Pos + 1;
            }
        }

        public int PositionOfEqualOrBefore(KeyType Key)
        {
            lock(this)
            {
                var Pos = KeysInfo.Keys.BinarySearch(Key).Index;
                if (Pos < 0)
                {
                    if (Pos == -1)
                        return Pos;
                    Pos = (Pos * -1) - 1;
                    if (Pos == KeysInfo.Keys.Length)
                        return KeysInfo.Keys.Length - 1;
                }
                if (Pos < 0)
                    Pos = (Pos * -1) - 1;
                return Pos;
            }
        }

        public int Length { get => KeysInfo.Keys.Length; }
    }
}
