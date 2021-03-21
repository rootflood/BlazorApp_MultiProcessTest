using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monsajem_Incs.Net.Base.Service;
using Monsajem_Incs.Database;
using System.Linq;
using Monsajem_Incs.Array.Hyper;
using System.Linq.Expressions;
using Monsajem_Incs.Serialization;
using System.Net.Http;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {
        public static async Task<bool> GetUpdate<ValueType, KeyType>(
            this Uri CDN,
            Table<ValueType, KeyType> Table,
            Action<ValueType> MakeingUpdate = null)
            where KeyType : IComparable<KeyType>
        {

            if (typeof(PartOfTable<ValueType, KeyType>).IsAssignableFrom(Table.GetType()))
                throw new Exception("Type of Main Table is (part of table) but expected orginal Table.");
            if (Table.TableName == null)
                throw new Exception("Table Name not found!");

            CDN = new Uri($"{CDN.ToString()}/{Table.TableName}");
            var Socket = new Net.Virtual.Socket();
            var Server = new Net.Virtual.AsyncOprations(Socket);
            var Client = new Net.Virtual.AsyncOprations(Socket.OtherSide);

            var WebClient = new HttpClient();
            var ServerTable = (await WebClient.GetByteArrayAsync(CDN.ToString() + "/K")).Deserialize<KeyValue.Base.Table<ValueType, KeyType>>();
            ServerTable.ClearRelations = Table.ClearRelations;

            if (ServerTable.UpdateAble == null)
                throw new Exception("UpdateAble at Server Not Found!");

            var ServerTask = 
                Server.I_SendUpdate(ServerTable, ServerTable.UpdateAble.UpdateCodes,
                async (key) =>
                {
                    return (await WebClient.GetByteArrayAsync(CDN.ToString() + "/V/" +
                        Convert.ToBase64String(key.Serialize()))).Deserialize<ValueType>();
                }, false);

            var ClientTask = Client.I_GetUpdate(Table, MakeingUpdate, false);

            var R1 = await Task.WhenAny(ServerTask, ClientTask);

            if (R1.Id == ServerTask.Id)
                await ServerTask;
            else
                await ClientTask;

            return await ClientTask;
        }

        public static Task<bool> GetUpdate<ValueType_RLN, KeyType_RLN, ValueType, KeyType>(
            this Uri CDN,
            Table<ValueType_RLN, KeyType_RLN> RLNTable,
            KeyType_RLN RLNKey,
            Func<ValueType_RLN, PartOfTable<ValueType, KeyType>> GetRelation,
            Action<ValueType> MakeingUpdate = null)
            where KeyType : IComparable<KeyType>
            where KeyType_RLN : IComparable<KeyType_RLN>
        {
            var PartTable = GetRelation(RLNTable[RLNKey]);
            var RootCDN = new Uri($"{CDN.ToString()}/{PartTable.Parent.TableName}");
            var RelationCDN = new Uri($"{CDN.ToString()}/{RLNTable.TableName}");
            return GetUpdate(RootCDN, RelationCDN, RLNTable, RLNKey, GetRelation, MakeingUpdate);
        }

        private static async Task<bool> GetUpdate<ValueType_RLN, KeyType_RLN, ValueType, KeyType>(
            this Uri RootCDN,
            Uri RelationCDN,
            Table<ValueType_RLN, KeyType_RLN> RLNTable,
            KeyType_RLN RLNKey,
            Func<ValueType_RLN, PartOfTable<ValueType, KeyType>> GetRelation,
            Action<ValueType> MakeingUpdate)
            where KeyType : IComparable<KeyType>
            where KeyType_RLN : IComparable<KeyType_RLN>
        {
            if (typeof(PartOfTable<ValueType, KeyType>).IsAssignableFrom(RLNTable.GetType()))
                throw new Exception("Type of Main Table is (part of table) but expected orginal Table.");
            if (GetRelation == null)
                throw new Exception("Get Update in part of table need to known relation.");

            var Socket = new Net.Virtual.Socket();
            var Server = new Net.Virtual.AsyncOprations(Socket);
            var Client = new Net.Virtual.AsyncOprations(Socket.OtherSide);

            var WebClient = new HttpClient();
            var ServerTable = (await WebClient.GetByteArrayAsync(
                                RootCDN.ToString() + "/K")).Deserialize<KeyValue.Base.Table<ValueType, KeyType>>();
            var ServerPartTable =
                GetRelation((await WebClient.GetByteArrayAsync(
                    RelationCDN.ToString() + "/V/" + Convert.ToBase64String(RLNKey.Serialize()))).Deserialize<ValueType_RLN>());

            if (ServerPartTable == null)
                return false;
            ValueType_RLN RLNValue;
            try
            {
                RLNValue = RLNTable[RLNKey].Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Get Value of the key have exception when update.", ex);
            }

            var ClientTable = GetRelation(RLNValue);
            ServerTable.ClearRelations = ClientTable.Parent.ClearRelations;
            ServerPartTable.Parent = ServerTable;

            var ServerTask = Server.I_SendUpdate(ServerPartTable, ServerTable.UpdateAble.UpdateCodes,
                async (key) =>
                {
                    return (await WebClient.GetByteArrayAsync(RootCDN.ToString() + "/V/" +
                        Convert.ToBase64String(key.Serialize()))).Deserialize<ValueType>();
                }, true);

            var ClientTask = Client.I_GetUpdate(ClientTable, MakeingUpdate, true);

            var R1 = await Task.WhenAny(ServerTask, ClientTask);

            if (R1.Id == ServerTask.Id)
                await ServerTask;
            else
                await ClientTask;

            return await ClientTask;

        }
    }
}