using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monsajem_Incs.Net.Base.Service;
using Monsajem_Incs.Database;
using System.Linq;
using Monsajem_Incs.Array.Hyper;
using System.Linq.Expressions;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {
        private static (IEnumerable<UpdateAble<KeyType>> UpdateCodes, IEnumerable<Task<ValueType>> Values, ulong FirstCode)
            Base_GetNexts<KeyType, ValueType>(
            Action<Table<ValueType, KeyType>.ValueInfo> ClearRelations,
            UpdateAble<KeyType>[] UpdateCodes,
            Func<KeyType, Task<ValueType>> GetItem,
            ulong UpdateCode)
            where KeyType : IComparable<KeyType>
        {
            var Place = System.Array.BinarySearch(UpdateCodes,
                        new UpdateAble<KeyType>() { UpdateCode = UpdateCode },
                        UpdateAble<KeyType>.CompareCode);
            if (Place < 0)
                Place = (Place * -1) - 1;
            else
                Place += 1;

            ulong FirstCode = 0;
            if (Place > 0)
                FirstCode = UpdateCodes[Place - 1].UpdateCode;

            var ResultCodes = UpdateCodes.Skip(Place);
            var Values = ResultCodes.Select(async (c) =>
            {
                var Data = new Table<ValueType, KeyType>.ValueInfo() { Value = await GetItem(c.Key) };
                ClearRelations?.Invoke(Data);
                return Data.Value;
            });
            return (ResultCodes, Values, FirstCode);
        }
        private static (ulong[] UpdateCodes, IEnumerable<Task<ValueType>> Values, ulong FirstCode)
            GetNexts<KeyType, ValueType>(
            Action<Table<ValueType, KeyType>.ValueInfo> ClearRelations,
            UpdateAble<KeyType>[] UpdateCodes,
            Func<KeyType, Task<ValueType>> GetItem,
            ulong UpdateCode)
            where KeyType : IComparable<KeyType>
        {
            var Nexts = Base_GetNexts(ClearRelations, UpdateCodes, GetItem, UpdateCode);

            var Updates = Nexts.UpdateCodes.Select((c) => c.UpdateCode).ToArray();
            return (Updates, Nexts.Values, Nexts.FirstCode);
        }
        private static (ulong[] ParentCodes, ulong[] PartCodes, IEnumerable<Task<ValueType>> Values, ulong FirstCode)
            GetNexts<KeyType, ValueType>(
            PartOfTable<ValueType, KeyType> Table,
            Func<KeyType, Task<ValueType>> GetItem,
            ulong UpdateCode)
            where KeyType : IComparable<KeyType>
        {
            var Nexts = Base_GetNexts(Table.Parent.ClearRelations, Table.UpdateAble.UpdateCodes, GetItem, UpdateCode);
            var ParentUpdateAble = Table.Parent.UpdateAble;
            var ParentCodes = Nexts.UpdateCodes.Select((c) => ParentUpdateAble[c.Key].UpdateCode).ToArray();
            var PartCodes = Nexts.UpdateCodes.Select((c) => c.UpdateCode).ToArray();

            return (ParentCodes, PartCodes, Nexts.Values, Nexts.FirstCode);
        }

        private static async Task I_SendUpdate<ValueType, KeyType>
                   (this IAsyncOprations Client,
                    Table<ValueType, KeyType> Table,
                    UpdateAble<KeyType>[] UpdateCodes,
                    Func<KeyType, Task<ValueType>> GetItem,
                    bool IsPartOfTable)
                   where KeyType : IComparable<KeyType>
        {
            Table<ValueType, KeyType> ParentTable = Table;
            PartOfTable<ValueType, KeyType> PartTable = null;
            if (IsPartOfTable)
            {
                PartTable = (PartOfTable<ValueType, KeyType>)Table;
                ParentTable = PartTable.Parent;
            }

            var LastUpdateCode = await Client.GetData<ulong>();//1
            await Client.SendData(Table.UpdateAble.UpdateCode);//2
            if (Table.UpdateAble.UpdateCode < LastUpdateCode)
                LastUpdateCode = 0;
            if (Table.UpdateAble.UpdateCode != LastUpdateCode)
            {
                IEnumerator<Task<ValueType>> IE_Values;
                if (IsPartOfTable)
                {
                    var NextUpdates = GetNexts(PartTable, GetItem, LastUpdateCode);
                    await Client.SendData(NextUpdates.FirstCode);//3
                    while (await Client.GetData<int>() != -1)
                    {
                        var Key = await Client.GetData<KeyType>();
                        await Client.SendCondition(ParentTable.PositionOf(Key) >= 0);
                    }
                    await Client.SendData(NextUpdates.ParentCodes);//4
                    await Client.SendData(NextUpdates.PartCodes);//5
                    IE_Values = NextUpdates.Values.GetEnumerator();
                }
                else
                {
                    var NextUpdates = GetNexts(Table.ClearRelations, UpdateCodes, GetItem, LastUpdateCode);
                    await Client.SendData(NextUpdates.FirstCode);//3
                    await Client.SendData(NextUpdates.UpdateCodes);//4
                    IE_Values = NextUpdates.Values.GetEnumerator();
                }
                var SendToClient = await Client.GetData<bool[]>();//5

                IE_Values.MoveNext();
                var Len = SendToClient.Length;
                for (int i = 0; i < Len; i++)
                {
                    if (SendToClient[i] == true)
                        await Client.SendData(await IE_Values.Current);//6
                    IE_Values.MoveNext();
                }
                IE_Values.Dispose();
            }

            if (IsPartOfTable)
                UpdateCodes = PartTable.UpdateAble.UpdateCodes;
            await Client.SendData(UpdateCodes.Length);
            var Pos = await Client.GetData<int>();
            if (IsPartOfTable)
            {
                while (Pos != -1)
                {
                    if (Pos == -2)
                    {
                        var Key = await Client.GetData<KeyType>();
                        await Client.SendCondition(ParentTable.PositionOf(Key) >= 0);
                    }
                    else
                    {
                        await Client.SendData(UpdateCodes[Pos].UpdateCode);
                    }
                    Pos = await Client.GetData<int>();
                }
            }
            else
            {
                while (Pos != -1)
                {
                    await Client.SendData(UpdateCodes[Pos].UpdateCode);
                    Pos = await Client.GetData<int>();
                }
            }
        }

        private static async Task CompareToOther<KeyType, ValueType>(
            Func<int, Task<ulong>> Ar_SV,
            Table<ValueType, KeyType> Table,
            Func<KeyType, Task> Delete,
            int TrueLen)
            where KeyType : IComparable<KeyType>
        {
            while (Table.UpdateAble.UpdateCodes.Length != TrueLen)
            {
                var UpdateCodes = Table.UpdateAble.UpdateCodes;
                int EndPos = TrueLen - 1;
                int BeginPos = 0;
                int MidPos = (EndPos + BeginPos + 1) / 2;
                while (EndPos != BeginPos &&
                      EndPos != MidPos &&
                      BeginPos != MidPos)
                {
                    var E = await Ar_SV(EndPos) == UpdateCodes[EndPos].UpdateCode;
                    var B = await Ar_SV(BeginPos) == UpdateCodes[BeginPos].UpdateCode;
                    var M = await Ar_SV(MidPos) == UpdateCodes[MidPos].UpdateCode;
                    if (E == false && M == false && B == true)
                    {
                        EndPos = BeginPos - 1;
                        MidPos = (EndPos + BeginPos + 1) / 2;
                    }
                    else if (E == false && M == true && B == true)
                    {
                        BeginPos = MidPos + 1;
                        MidPos = (EndPos + BeginPos + 1) / 2;
                    }
                    else if (E == true && M == true && B == true)
                    {
                        EndPos = EndPos + 1;
                        MidPos = EndPos;
                        BeginPos = EndPos;
                    }
                    else if (E == false && M == false && B == false)
                    {
                        EndPos = BeginPos;
                        MidPos = BeginPos;
                    }
                }
                var Key = UpdateCodes[EndPos].Key;
                await Delete(Key);
            }
        }

        private static async Task DeleteFrom<KeyType, ValueType>(
            Table<ValueType, KeyType> Table,
            Func<KeyType, Task> Delete,
            ulong UpdateCode)
            where KeyType : IComparable<KeyType>
        {
            var UpdateCodes = Table.UpdateAble.UpdateCodes;
            var Place = System.Array.BinarySearch(UpdateCodes,
                        new UpdateAble<KeyType>() { UpdateCode = UpdateCode },
                        UpdateAble<KeyType>.CompareCode);
            if (Place < 0)
                Place = (Place * -1) - 1;
            else
                Place += 1;
            foreach (var Update in UpdateCodes.Skip(Place))
            {
                await Delete(Update.Key);
            }
        }
        private static async Task<bool> I_GetUpdate<DataType, KeyType>(
            this IAsyncOprations Client,
            Table<DataType, KeyType> Table,
            Action<DataType> MakeingUpdate,
            bool IsPartOfTable)
            where KeyType : IComparable<KeyType>
        {
            Table<DataType, KeyType> ParentTable = Table;
            PartOfTable<DataType, KeyType> PartTable = null;
            Func<KeyType, Task> Delete;
            if (Table._UpdateAble == null)
                Table._UpdateAble = new UpdateAbles<KeyType>(0);

            if (IsPartOfTable)
            {
                PartTable = (PartOfTable<DataType, KeyType>)Table;
                ParentTable = PartTable.Parent;
                if (ParentTable._UpdateAble == null)
                    ParentTable._UpdateAble = new UpdateAbles<KeyType>(0);
                var Part_UpdateAble = PartTable.UpdateAble;
                var Parent_UpdateAble = ParentTable.UpdateAble;
                Delete = async (key) =>
                {
                    await Client.SendData(-2);
                    await Client.SendData(key);
                    Part_UpdateAble.DeleteDontUpdate(key);
                    if (await Client.GetCondition())
                        PartTable.Ignore(key);
                    else
                    {
                        Parent_UpdateAble.DeleteDontUpdate(key);
                        PartTable.Delete(key);
                    }
                };
            }
            else
            {
                var UpdateAble = Table.UpdateAble;
                Delete = async (key) =>
                {
                    UpdateAble.DeleteDontUpdate(key);
                    Table.Delete(key);
                };
            }

            var Result = false;

            await Client.SendData(Table.UpdateAble.UpdateCode);//1
            var LastUpdateCode = await Client.GetData<ulong>();//2
            if (Table.UpdateAble.UpdateCode > LastUpdateCode)
            {
                Table.UpdateAble.UpdateCode = 0;
                Table.UpdateAble.UpdateCodes = new UpdateAble<KeyType>[0];
                Table.UpdateAble.UpdateKeys = new UpdateAble<KeyType>[0];
                if (IsPartOfTable)
                {
                    ParentTable.UpdateAble.UpdateCode = 0;
                    ParentTable.UpdateAble.UpdateCodes = new UpdateAble<KeyType>[0];
                    ParentTable.UpdateAble.UpdateKeys = new UpdateAble<KeyType>[0];
                }
            }
            if (Table.UpdateAble.UpdateCode != LastUpdateCode)
            {
                await DeleteFrom(Table, Delete, await Client.GetData<ulong>());//3
                if (IsPartOfTable)
                    await Client.SendData(-1);

                bool[] NeedUpdate;
                var UpdateCodes = await Client.GetData<ulong[]>();//4
                ulong[] PartUpdateCodes = null;
                if (IsPartOfTable)
                    PartUpdateCodes = await Client.GetData<ulong[]>();//5

                NeedUpdate = new bool[UpdateCodes.Length];

                if (IsPartOfTable)
                    for (int i = 0; i < UpdateCodes.Length; i++)
                        NeedUpdate[i] = ParentTable.UpdateAble.IsExist(UpdateCodes[i]) == false;
                else
                    for (int i = 0; i < UpdateCodes.Length; i++)
                        NeedUpdate[i] = Table.UpdateAble.IsExist(UpdateCodes[i]) == false;

                await Client.SendData(NeedUpdate);//5

                for (int i = 0; i < NeedUpdate.Length; i++)
                {
                    if (NeedUpdate[i] == true)
                    {
                        var Data = await Client.GetData<DataType>();//6
                        MakeingUpdate?.Invoke(Data);
                        var Key = Table.GetKey(Data);
                        if (ParentTable.PositionOf(Key) > -1)
                        {
                            ParentTable.Update(Key, (c) =>
                            {
                                ParentTable.MoveRelations(c, Data);
                                return Data;
                            });
                            ParentTable.UpdateAble.Changed(Key, Key, UpdateCodes[i]);
                        }
                        else
                        {
                            ParentTable.Insert(Data);
                            ParentTable.UpdateAble.Insert(Key, UpdateCodes[i]);
                        }
                        if (IsPartOfTable)
                        {
                            if (PartTable.PositionOf(Key) > -1)
                            {
                                PartTable.UpdateAble.Changed(Key, Key, PartUpdateCodes[i]);
                            }
                            else
                            {
                                PartTable.UpdateAble.Insert(Key, PartUpdateCodes[i]);
                                PartTable.Accept(Key);
                            }
                        }
                    }
                    else if (IsPartOfTable)
                    {
                        var Data = ParentTable[ParentTable.UpdateAble[UpdateCodes[i]].Key];//6
                        MakeingUpdate?.Invoke(Data);
                        var Key = Table.GetKey(Data);
                        if (PartTable.PositionOf(Key) > -1)
                            PartTable.UpdateAble.Changed(Key, Key, PartUpdateCodes[i]);
                        else
                        {
                            PartTable.UpdateAble.Insert(Key, PartUpdateCodes[i]);
                            PartTable.Accept(Key);
                        }
                    }
                    Table.UpdateAble.UpdateCode = UpdateCodes[i];
                    Result = true;
                }
            }

            var TrueLen = await Client.GetData<int>();//6
            if (Table.UpdateAble.UpdateCodes.Length != TrueLen)
            {
                var ServerValues = new ulong?[TrueLen];
                await CompareToOther(async (pos) =>
                {
                    var Value = ServerValues[pos];
                    if (Value == null)
                    {
                        await Client.SendData(pos);//7
                        Value = await Client.GetData<ulong>();//8
                        ServerValues[pos] = Value;
                    }
                    return Value.Value;
                }, Table, Delete, TrueLen);
            }
            await Client.SendData(-1);//7+


            Table.UpdateAble.UpdateCode = LastUpdateCode;
            if (IsPartOfTable)
                PartTable.SaveToParent();
            return Result;
        }
    }
}
