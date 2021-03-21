using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static Monsajem_Incs.ArrayExtentions.ArrayReturns;
using Monsajem_Incs.Serialization;
using static Monsajem_Incs.DynamicAssembly.DelegatesExtentions;
using static Monsajem_Incs.Database.Base.Runer;

namespace Monsajem_Incs.Database.Base
{
    public abstract class RelationJoined
    {
        public static Func<RelationJoined> Make;
        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            ValueTuple<Table<R1ValueType, R1KeyType>.RelationItemInfo<R2ValueType, R2KeyType>,
                       Table<R2ValueType, R2KeyType>.RelationItemInfo<R1ValueType, R1KeyType>> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            ValueTuple<Table<R1ValueType, R1KeyType>.RelationTableInfo<R2ValueType, R2KeyType>,
                       Table<R2ValueType, R2KeyType>.RelationItemInfo<R1ValueType, R1KeyType>> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            ValueTuple<Table<R1ValueType, R1KeyType>.RelationItemInfo<R2ValueType, R2KeyType>,
                       Table<R2ValueType, R2KeyType>.RelationTableInfo<R1ValueType, R1KeyType>> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            ValueTuple<Table<R1ValueType, R1KeyType>.RelationTableInfo<R2ValueType, R2KeyType>,
                       Table<R2ValueType, R2KeyType>.RelationTableInfo<R1ValueType, R1KeyType>> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            Table<R1ValueType, R1KeyType>.RelationTableInfo<R2ValueType, R2KeyType> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

        public abstract void OnJoin<R1ValueType, R1KeyType, R2ValueType, R2KeyType>(
            Table<R1ValueType, R1KeyType>.RelationItemInfo<R2ValueType, R2KeyType> Relation)
            where R1KeyType : IComparable<R1KeyType>
            where R2KeyType : IComparable<R2KeyType>;

    }

    public partial class Table<ValueType, KeyType>
    {
        public struct RelationItem
        {
            [Monsajem_Incs.Serialization.NonSerialized]
            private Table<ValueType, KeyType> P_Array;
            public Table<ValueType, KeyType> Array
            {
                get => P_Array;
                set
                {
                    this.P_Array = value;
                    if (P_GetValue != null)
                    {
                        Value = P_GetValue();
                        P_GetValue = null;
                    }
                }
            }
            public object Key;
            [Monsajem_Incs.Serialization.NonSerialized]
            public object OldKey;
            [Monsajem_Incs.Serialization.NonSerialized]
            private Func<ValueType> P_GetValue;
            public ValueType Value
            {
                get => Array.GetItem((KeyType)Key).Value;
                set
                {
                    Key = ((Table<ValueType, KeyType>)Array).GetKey(value);
                }
            }

            public static implicit operator ValueType(RelationItem RL)
            {
                return RL.Value;
            }

            public static implicit operator RelationItem(ValueType Value)
            {
                return new RelationItem() { P_GetValue = () => Value };
            }

            public override string ToString()
            {
                return "RelationItem Of " + typeof(ValueType).ToString();
            }
        }

        private int Compare(object k1, object k2)
        {
            if (k1 == null)
                if (k2 == null)
                    return 0;
                else
                    return int.MinValue;
            if (k2 == null)
                return int.MaxValue;
            return ((KeyType)k1).CompareTo((KeyType)k2);
        }

        [Serialization.NonSerialized]
        public Action<ValueInfo> ClearRelations;
        [Serialization.NonSerialized]
        public Action<ValueType, ValueType> MoveRelations;

        private void _ClearRelation<To, ToKeyType>(
            Expression<Func<ValueType, PartOfTable<To, ToKeyType>>> RelationLink)
            where ToKeyType : IComparable<ToKeyType>
        {
            var Fild = DynamicAssembly.FieldControler.Make(RelationLink);
            this.ClearRelations += (ValueInfo info) =>
            {
                Fild.Value(info.Value,()=> null);
            };
            this.MoveRelations += (ValueType Frominfo, ValueType Toinfo) =>
            {
                Fild.Value(Toinfo,()=>Fild.Value(Frominfo));
            };
        }

        private void _ClearRelation<To, ToKeyType>(
            Expression<Func<ValueType, PartOfTable<To, ToKeyType>.RelationItem>> RelationLink)
            where ToKeyType : IComparable<ToKeyType>
        {
            var Fild = DynamicAssembly.FieldControler.Make(RelationLink);
            this.ClearRelations += (ValueInfo info) =>
            {
                Fild.Value(info.Value, (c) => { c.Key = null; return c; });
            };
            this.MoveRelations += (ValueType Frominfo, ValueType Toinfo) =>
            {
                Fild.Value(Toinfo,()=>Fild.Value(Frominfo));
            };
        }

        private void _MoveRelations<To, ToKeyType>(
            Expression<Func<ValueType, PartOfTable<To, ToKeyType>.RelationItem>> RelationLink)
            where ToKeyType : IComparable<ToKeyType>
        {
            var Fild = DynamicAssembly.FieldControler.Make(RelationLink);
            this.MoveRelations += (ValueType Frominfo, ValueType Toinfo) =>
            {
                Fild.Value(Toinfo,(ToField)=>{
                    var FromField = Fild.Value(Frominfo);
                    if (ToField.Key == null)
                    {
                        if (FromField.Key == null)
                            ToField.OldKey = ToField.Key;
                    }
                    else if (FromField.Key != null)
                        if (((ToKeyType)FromField.Key).CompareTo((ToKeyType)ToField.Key) == 0)
                            ToField.OldKey = ToField.Key;
                    return ToField;
                });
            };
        }

        private void _AddRelationForLoading<To, ToKeyType>(
            string RelationName,
            RelationTableInfo<To, ToKeyType> Relation,
            Action<KeyType, ToKeyType, PartOfTable<To,ToKeyType>> Accepted,
            Action<KeyType, ToKeyType, PartOfTable<To, ToKeyType>> Ignored,
            Action<(
                ValueType LoadedValue,
                To NewValue)> MakeNew=null)
            where ToKeyType : IComparable<ToKeyType>
        {
            var Fild = DynamicAssembly.FieldControler.Make(Relation.Link);

            this.Events.loading += (Value) =>
            {
               Fild.Value(Value,(ThisRelation)=>
               {
                   var Key = this.GetKey(Value);

                   if (ThisRelation == null)
                       ThisRelation = new PartOfTable<To, ToKeyType>(new ToKeyType[0], Relation.LinkArray)
                       {
                           AutoFillRelations = (c) =>
                           {
                               Relation.LinkArray.AutoFillRelations?.Invoke(c);
                               MakeNew?.Invoke((Value, c));
                           }
                       };
                   else if (ThisRelation.Extras == null)
                   {
                       ThisRelation = new PartOfTable<To, ToKeyType>(ThisRelation.KeysInfo.Keys, Relation.LinkArray)
                       {
                           _UpdateAble = ThisRelation._UpdateAble,
                           AutoFillRelations = (c) =>
                           {
                               Relation.LinkArray.AutoFillRelations?.Invoke(c);
                               MakeNew?.Invoke((Value, c));
                           }
                       };
                       if (Relation.IsUpdateAble)
                           ThisRelation.ReadyForUpdateAble();
                   }
                   else
                       return ThisRelation;

                   ThisRelation.SaveToParent = () =>
                        this.Update(Key, (c) => Relation.Field.Value(c, (f) => ThisRelation));

                   ThisRelation.Extras.Accepted += (PartOfTable<To, ToKeyType>.TableExtras.KeyInfo Info) =>
                   {
                       if (Run.Use(RelationName))
                       {
                           Accepted(Key, Info.Key,ThisRelation);
                       }
                   };

                   ThisRelation.Extras.Ignored += (PartOfTable<To, ToKeyType>.TableExtras.KeyInfo Info) =>
                   {
                       if (Run.Use(RelationName))
                       {
                           Ignored(Key, Info.Key, ThisRelation);
                       }
                   };

                   if (Relation.IsUpdateAble)
                       if (ThisRelation.UpdateAble == null)
                           ThisRelation.UpdateAble = new UpdateAbles<ToKeyType>();
                   return ThisRelation;
               });
            };
        }

        public void _AddRelationForLoading<To, ToKeyType>(
            string RelationName,
            RelationItemInfo<To, ToKeyType> Relation)
            where ToKeyType : IComparable<ToKeyType>
        {
            var Fild = DynamicAssembly.FieldControler.Make(Relation.Link);

            this.Events.loading += (Value) =>
            {
                    Fild.Value(Value,(ThisRelation)=>
                    {
                        if (ThisRelation.Array == null)
                        {
                            ThisRelation.Array = Relation.LinkArray;
                            ThisRelation.OldKey = (ToKeyType)ThisRelation.Key;
                        }
                        return ThisRelation;
                    });
            };
        }
    }
}
