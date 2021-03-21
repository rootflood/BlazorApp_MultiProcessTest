using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static Monsajem_Incs.ArrayExtentions.ArrayReturns;
using static Monsajem_Incs.DynamicAssembly.DelegatesExtentions;
using static Monsajem_Incs.Database.Base.Runer;

namespace Monsajem_Incs.Database.Base
{
    public partial class Table<ValueType, KeyType>
    {
        public void AddRelation<To, ToKeyType>(
            RelationItemInfo<To, ToKeyType> ThisRelationLink,
            Table<To,ToKeyType>.RelationItemInfo<ValueType,KeyType> ThatRelationLink)
            where ToKeyType : IComparable<ToKeyType>
        {
            var RelationName = ThisRelationLink.Link.Body.ToString() + ThatRelationLink.Link.Body.ToString();
            this._AddRelation(RelationName, ThisRelationLink, ThatRelationLink);
            ThisRelationLink.LinkArray._AddRelation(RelationName, ThatRelationLink, ThisRelationLink);
        }

        private void _AddRelation<To, ToKeyType>(
            string RelationName,
            RelationItemInfo<To, ToKeyType> ThisRelationLink,
            Table<To,ToKeyType>.RelationItemInfo<ValueType,KeyType> ThatRelationLink)
            where ToKeyType : IComparable<ToKeyType>
        {
#if TRACE
            Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_1");
#endif
            this.Events.loading += (Value) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_1 >> loading");
#endif
                var ThisValue = ThisRelationLink.Field.Value(Value);
                
                if (ThisValue.Array == null)
                {
                    ThisValue.Array = ThisRelationLink.LinkArray;
                    ThisValue.OldKey = ThisValue.Key;
                }
                ThisRelationLink.Field.Value(Value, ()=> (Table<To, ToKeyType>.RelationItem)ThisValue);
            };

            this.KeyChanged += (info) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_1 >> KeyChanged");
#endif
                if (Run.Use(RelationName))
                {
                    var ThisValue = ThisRelationLink.Field.Value(info.Value);
                    if (ThisValue.Key != null)
                    {
                        var ThatValue = ThisValue.Value;
                        var ThatRelation = ThatRelationLink.Field.Value(ThatValue);
                        ThatRelation.Key = info.NewKey;
                        ThisRelationLink.LinkArray.Update(ThatValue);
                    }
                }
            };

            if (!ThisRelationLink.IsChild)
            {
                this.Events.Deleted += (Value) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_1 >> Deleted IsChild");
#endif
                    if (Run.Use(RelationName))
                    {
                        var ThisRelation = ThisRelationLink.Field.Value(Value.Value);
                        var Key = ThisRelation.Key;
                        if (Key != null)
                        {
                            ThisRelationLink.LinkArray.Delete((ToKeyType)Key);
                        }
                    }
                };
            }
            else
            {
                this.Events.Deleted += (Value) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_1 >> Deleted");
#endif
                    if (Run.Use(RelationName))
                    {
                        var ThisRelation = ThisRelationLink.Field.Value(Value.Value);
                        var Key = ThisRelation.Key;
                        if (Key != null)
                        {
                            ThisRelationLink.LinkArray.Update((ToKeyType)Key,
                                    (c) => ThatRelationLink.Field.Value(c,(f)=> { f.Key = null; return f; }));
                        }
                    }
                };
            }

            Action<(ValueType Value,Events<ValueType>.ValueInfo[] Info)> OnSave = (Value) =>
            {
                if (Run.Use(RelationName))
                {
                    var ThisRelation = ThisRelationLink.Field.Value(Value.Value);
                    var Key = ThisRelation.Key;
                    var OldKey = ThisRelation.OldKey;
                    if (Compare(Key, OldKey) != 0)
                    {
                        if (Key != null)
                        {
                            ThisRelationLink.LinkArray.Update((ToKeyType)Key,
                                (c) => ThatRelationLink.Field.Value(c,(f)=> { f.Key = this.GetKey(Value.Value);return f;}));
                        }
                        if (OldKey != null)
                        {
                            ThisRelationLink.LinkArray.Update((ToKeyType)OldKey,
                                (c) => ThatRelationLink.Field.Value(c,(f)=>{ f.Key = null; return f; }));
                        }
                    }
                }
            };

            this.Events.Updated += (Value) => OnSave(Value);
            this.Events.Inserted += (Value) => OnSave(Value);

            ThisRelationLink.OwnerArray._MoveRelations(ThisRelationLink.Link);
            ThatRelationLink.OwnerArray._MoveRelations(ThatRelationLink.Link);

            if (ThisRelationLink.ClearRelationOnSendUpdate)
                ThisRelationLink.OwnerArray._ClearRelation(ThisRelationLink.Link);
            if (ThatRelationLink.ClearRelationOnSendUpdate)
                ThatRelationLink.OwnerArray._ClearRelation(ThatRelationLink.Link);
        }
    }
}