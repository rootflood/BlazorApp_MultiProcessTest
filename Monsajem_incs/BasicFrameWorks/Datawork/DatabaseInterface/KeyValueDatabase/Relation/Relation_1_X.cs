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
            RelationItemInfo<To, ToKeyType> ThisRelation,
            Table<To, ToKeyType>.RelationTableInfo<ValueType, KeyType> ThatRelation)
            where ToKeyType : IComparable<ToKeyType>
        {
#if TRACE
            Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X");
#endif
            var RelationName = ThisRelation.Link.Body.ToString() + ThatRelation.Link.Body.ToString();

            this._AddRelationForLoading(RelationName,
                ThisRelation);

            ThisRelation.LinkArray._AddRelationForLoading(RelationName,
                ThatRelation,
                Accepted: (Key, AcceptedKey,PartTable) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Accepted");
#endif
                    using (ThisRelation.LinkArray.IgnoreUpdateAble.Block())
                    {
                        PartTable.SaveToParent();
                    }

                    using (this.IgnoreUpdateAble.Block())
                    {
                        this.Update(AcceptedKey,
                             (c) => ThisRelation.Field.Value(c,(f)=> { f.Key = Key; return f; }));
                    }
                },
                Ignored: (Key, IgnoredKey, PartTable) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Ignored");
#endif
                    using (ThisRelation.LinkArray.IgnoreUpdateAble.Block())
                    {
                        PartTable.SaveToParent();
                    }


                    using (this.IgnoreUpdateAble.Block())
                    {
                        if (this.PositionOf(IgnoredKey) > -1)
                            this.Update(IgnoredKey,
                                (c) => ThisRelation.Field.Value(c,(f)=> { f.Key = null; return f; }));
                    }
                },
                MakeNew:(c)=>
                {
                    ThisRelation.Field.Value(c.NewValue, (f) =>
                     {
                         f.Key = ThatRelation.OwnerArray.GetKey(c.LoadedValue);
                         return f;
                     });
                });

            this.KeyChanged += (info) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> KeyChanged");
#endif
                if (Run.Use(RelationName))
                {
                    using (ThisRelation.LinkArray.IgnoreUpdateAble.Block())
                    {
                        var ThisRelationArray = ThisRelation.Field.Value(info.Value);
                        if (ThisRelationArray.Key != null)
                        {
                            ThisRelation.LinkArray.Update((ToKeyType)ThisRelationArray.Key,
                                (c) =>
                                {
                                    ThatRelation.Field.Value(c).Ignore(info.OldKey);
                                    ThatRelation.Field.Value(c).Accept(info.NewKey);
                                });
                        }
                    }
                }
            };

            ThisRelation.LinkArray.KeyChanged += (info) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> KeyChanged");
#endif
                if (Run.Use(RelationName))
                {
                    using (this.IgnoreUpdateAble.Block())
                    {
                        var ThatRelation_array = ThatRelation.Field.Value(info.Value);
                        for (int i = 0; i < ThatRelation_array.KeysInfo.Keys.Length; i++)
                        {
                            var ThisValue = ThatRelation_array[i];
                            var Key = this.GetKey(ThisValue);
                            ThisRelation.Field.Value(ThisValue.Value, (f) => { f.Key = info.NewKey; return f; });
                            ThisRelation.Field.Value(ThisValue.Value, (f) => { f.OldKey = info.NewKey; return f; });
                            this.Update(Key, ThisValue);
                        }
                    }
                }
            };

            this.Events.Deleted += (Info) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Deleted");
#endif
                if (Run.Use(RelationName))
                {
                    using (ThisRelation.LinkArray.IgnoreUpdateAble.Block())
                    {
                        var ThisRelationArray = ThisRelation.Field.Value(Info.Value);
                        if (ThisRelationArray.Key != null)
                        {
                            ThisRelation.LinkArray.Update((ToKeyType)ThisRelationArray.Key,
                                (c) =>
                                {
                                    ThatRelation.Field.Value(c).Ignore((KeyType)Info.Info[KeyPos].Key);
                                });
                        }
                    }
                }
            };

            if (ThisRelation.IsChild)
            {
                ThisRelation.LinkArray.Events.Deleted += (info) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Deleted IsChild");
#endif
                    if (Run.Use(RelationName))
                    {
                        var ThatRelation_array = ThatRelation.Field.Value(info.Value);
                        foreach (var ThisKey in ThatRelation_array.KeysInfo.Keys)
                        {
                            this.Delete(ThisKey);
                        }
                    }
                };
            }
            else
            {
                ThisRelation.LinkArray.Events.Deleted += (info) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Deleted");
#endif
                    if (Run.Use(RelationName))
                    {
                        var ThatRelation_array = ThatRelation.Field.Value(info.Value);
                        for (int i = 0; i < ThatRelation_array.KeysInfo.Keys.Length; i++)
                        {
                            var ThisValue = ThatRelation_array[i];
                            var Key = this.GetKey(ThisValue);
                            ThisRelation.Field.Value(ThisValue.Value, (f) => { f.Key = null; return f; });
                            this.Update(Key, ThisValue);
                        }
                    }
                };
            }

            Action<(ValueType Value,Events<ValueType>.ValueInfo[] Info)> OnSave = (Value) =>
            {
#if TRACE
                Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> OnSave");
#endif
                if (Run.Use(RelationName))
                {
#if TRACE
                    Console.WriteLine(">> var ThisRelationArray = ThisRelation.Field.Value(ref Value.Value);");
#endif
                    var ThisRelationArray = ThisRelation.Field.Value(Value.Value);
                    var Key = ThisRelationArray.Key;
                    var OldKey = ThisRelationArray.OldKey;
                    if (Compare(Key, OldKey) != 0)
                    {
                        var ChangedKey = this.GetKey(Value.Value);
                        if (Key != null)
                        {
                            ThisRelation.LinkArray.Update((ToKeyType)ThisRelationArray.Key,
                            (c) =>
                            {
#if TRACE
                                Console.WriteLine(">> Key != null");
#endif
                                ThatRelation.Field.Value(c).Accept(ChangedKey);
                            });
                        }
                        if (OldKey != null)
                        {
#if TRACE
                            Console.WriteLine(">> OldKey != null");
#endif
                            ThisRelation.LinkArray.Update((ToKeyType)ThisRelationArray.Key,
                            (c) =>
                            {
                                ThatRelation.Field.Value(c).Ignore(ChangedKey);
                            });
                        }
                    }
                }
            };

            this.Events.Inserted += (Value) =>OnSave(Value);
            this.Events.Updated += (Value) => OnSave(Value);

            if (ThatRelation.IsUpdateAble)
            {
                this.Events.Updated += (info) =>
                {
#if TRACE
                    Console.WriteLine("@ " + this.GetType().Namespace + this.GetType().Name + " _AddRelation_1_X >> Updated");
#endif
                    if (this.IgnoreUpdateAble.BlockLengths == 0 )
                    {
                        var ThatValue = ThisRelation.Field.Value(info.Value);
                        if (ThatValue.Key != null)
                            if (Run.Use(RelationName + "UP"))
                            {
                                using (ThisRelation.LinkArray.IgnoreUpdateAble.Block())
                                {

                                    ThisRelation.LinkArray.Update(ThatValue, (c) =>
                                    {
                                        var MyInfo = info.Info[KeyPos];
                                        ThatRelation.Field.Value(c).UpdateAble.Changed(
                                            (KeyType)MyInfo.OldKey, (KeyType)MyInfo.OldKey);
                                    });
                                }
                            }
                    }
                };
            }


            ThisRelation.LinkArray.Events.Saving += (Value) =>
            {

            };
            if (ThatRelation.ClearRelationOnSendUpdate)
                ThisRelation.LinkArray._ClearRelation(ThatRelation.Link);
            ThisRelation.OwnerArray._MoveRelations(ThisRelation.Link);
        }

        public void AddRelation<To, ToKeyType>(
            Table<To, ToKeyType>.RelationTableInfo<ValueType, KeyType> ThisRelation,
            RelationItemInfo<To, ToKeyType> ThatRelation)
            where ToKeyType : IComparable<ToKeyType>
        {
            ThisRelation.LinkArray.AddRelation(ThatRelation, ThisRelation);
        }
    }
}
