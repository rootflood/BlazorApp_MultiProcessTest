using System;
using System.IO;
using System.Linq.Expressions;
using Monsajem_Incs.StreamCollection;
using Monsajem_Incs.Serialization;
using Monsajem_Incs.Database.Base;
using Monsajem_Incs.DynamicAssembly;
namespace Monsajem_Incs.Database.DirectoryTable
{
    public class DirectoryFilesTable<ValueType,KeyType>:
        Table<ValueType,KeyType>
        where KeyType:IComparable<KeyType>
    {
        [Serialization.NonSerialized]
        private volatile bool NeedToSave = true;

        public DirectoryFilesTable(
            string DirectoryAddress,
            Func<ValueType, KeyType> GetKey,
            bool IsUpdateAble,
            bool FastSave):
            base(new StreamCollection.StreamCollection<ValueType>(),GetKey,false)
        {
            this.TableName = new DirectoryInfo(DirectoryAddress).Name;
            Directory.CreateDirectory(DirectoryAddress);
            Directory.CreateDirectory(DirectoryAddress+"\\Keys");
            if (File.Exists(DirectoryAddress + "\\Keys\\PK"))
            {
                var OldTable = File.ReadAllBytes(DirectoryAddress + "\\PK").Deserialize(this);
                this.KeysInfo.Keys = OldTable.KeysInfo.Keys;
                if (IsUpdateAble)
                {
                    ReadyForUpdateAble();
                }
                this.UpdateAble = OldTable.UpdateAble;
            }
            else
            {
                if (IsUpdateAble)
                {
                    ReadyForUpdateAble();
                    this.UpdateAble = new UpdateAbles<KeyType>();
                }
            }

            var Stream_PK = File.Open(DirectoryAddress + "\\Keys\\PK", FileMode.OpenOrCreate);
            this.Serialize();

            Action Save = () =>
            {
                var sr = this.Serialize();
                Stream_PK.Seek(0, SeekOrigin.Begin);
                Stream_PK.SetLength(sr.Length);
                Stream_PK.Write(sr, 0, sr.Length);
                Stream_PK.FlushAsync();
            };

            if(FastSave==true)
            {
                var Save_trd = new System.Threading.Thread(() =>
                {
                    save:
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch
                    {
                        goto save;
                    }
                        if (this.NeedToSave == true)
                        {
                            Save();
                            NeedToSave = false;
                        }
                    goto save;
                });
                Save_trd.Start();

                this.Events.Inserted += (info) => {

                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                };
                this.Events.Deleted += (info) =>
                {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                };
                this.Events.Updated += (info) => {
                        if (this.NeedToSave == false)
                            this.NeedToSave = true;
                };
            }
            else
            {
               Runer.Run.OnEndBlocks+=()=>Save();
            }
        }
    }
}