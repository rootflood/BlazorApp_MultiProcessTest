using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static Monsajem_Incs.ArrayExtentions.ArrayReturns;
using Monsajem_Incs.Serialization;
using static Monsajem_Incs.DynamicAssembly.DelegatesExtentions;
using System.Linq;
using static Monsajem_Incs.Database.Base.Runer;

namespace Monsajem_Incs.Database.Base
{
    public static partial class Extentions
    {
        public static void Join<ValueType, KeyType, RelationValueType, RelationKeyType>(
            this ValueTuple<Table<RelationValueType, RelationKeyType>.RelationItemInfo<ValueType, KeyType>,
                            Table<ValueType, KeyType>.RelationItemInfo<RelationValueType, RelationKeyType>> Relation)
            where KeyType : IComparable<KeyType>
            where RelationKeyType : IComparable<RelationKeyType>
        {
            Relation.Item1.LinkArray = Relation.Item2.OwnerArray;
            Relation.Item2.LinkArray = Relation.Item1.OwnerArray;
            Relation.Item1.Field = DynamicAssembly.FieldControler.Make(Relation.Item1.Link);
            Relation.Item2.Field = DynamicAssembly.FieldControler.Make(Relation.Item2.Link);
            Relation.Item2.LinkArray.AddRelation(Relation.Item1, Relation.Item2);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }

        public static void Join<ValueType, KeyType>(
            this Table<ValueType, KeyType>.RelationItemInfo<ValueType, KeyType> Relation)
            where KeyType : IComparable<KeyType>
        {
            Relation.LinkArray = Relation.OwnerArray;
            Relation.Field = DynamicAssembly.FieldControler.Make(Relation.Link);
            Relation.LinkArray.AddRelation(Relation);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }

        public static void Join<ValueType, KeyType, RelationValueType, RelationKeyType>(
            this ValueTuple<Table<RelationValueType, RelationKeyType>.RelationTableInfo<ValueType, KeyType>,
                            Table<ValueType, KeyType>.RelationItemInfo<RelationValueType, RelationKeyType>> Relation)
            where KeyType : IComparable<KeyType>
            where RelationKeyType : IComparable<RelationKeyType>
        {
            Relation.Item1.LinkArray = Relation.Item2.OwnerArray;
            Relation.Item2.LinkArray = Relation.Item1.OwnerArray;
            Relation.Item1.Field = DynamicAssembly.FieldControler.Make(Relation.Item1.Link);
            Relation.Item2.Field = DynamicAssembly.FieldControler.Make(Relation.Item2.Link);
            Relation.Item1.LinkArray.AddRelation(Relation.Item1, Relation.Item2);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }
        public static void Join<ValueType, KeyType, RelationValueType, RelationKeyType>(
            this ValueTuple<Table<RelationValueType, RelationKeyType>.RelationItemInfo<ValueType, KeyType>,
                            Table<ValueType, KeyType>.RelationTableInfo<RelationValueType, RelationKeyType>> Relation)
            where KeyType : IComparable<KeyType>
            where RelationKeyType : IComparable<RelationKeyType>
        {
            Relation.Item1.LinkArray = Relation.Item2.OwnerArray;
            Relation.Item2.LinkArray = Relation.Item1.OwnerArray;
            Relation.Item1.Field = DynamicAssembly.FieldControler.Make(Relation.Item1.Link);
            Relation.Item2.Field = DynamicAssembly.FieldControler.Make(Relation.Item2.Link);
            Relation.Item2.LinkArray.AddRelation(Relation.Item1, Relation.Item2);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }

        public static void Join<ValueType, KeyType, RelationValueType, RelationKeyType>(
            this ValueTuple<Table<RelationValueType, RelationKeyType>.RelationTableInfo<ValueType, KeyType>,
                            Table<ValueType, KeyType>.RelationTableInfo<RelationValueType, RelationKeyType>> Relation)
            where KeyType : IComparable<KeyType>
            where RelationKeyType : IComparable<RelationKeyType>
        {
            Relation.Item1.LinkArray = Relation.Item2.OwnerArray;
            Relation.Item2.LinkArray = Relation.Item1.OwnerArray;
            Relation.Item1.Field = DynamicAssembly.FieldControler.Make(Relation.Item1.Link);
            Relation.Item2.Field = DynamicAssembly.FieldControler.Make(Relation.Item2.Link);
            Relation.Item2.LinkArray.AddRelation(Relation.Item1, Relation.Item2);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }

        public static void Join<ValueType, KeyType>(
            this Table<ValueType,KeyType>.RelationTableInfo<ValueType, KeyType> Relation)
            where KeyType : IComparable<KeyType>
        {
            Relation.LinkArray = Relation.OwnerArray;
            Relation.Field = DynamicAssembly.FieldControler.Make(Relation.Link);
            Relation.LinkArray.AddRelation(Relation);
            RelationJoined.Make?.Invoke().OnJoin(Relation);
        }
    }

    public static class DefaultRelationConfigs
    {
        public static Action<RelationItemInfo> DefaultItemInfo;
        public static Action<RelationTableInfo> DefaultTableInfo;
    }

    public class RelationTableInfo
    {
        public bool IsUpdateAble;
        public bool ClearRelationOnSendUpdate = true;
        public static void ReadyFill(){_ReadyFill?.Invoke(); }
        internal static Action _ReadyFill;
    }
    public class RelationItemInfo
    {
        public bool IsChild;
        public bool ClearRelationOnSendUpdate = false;
        public static void ReadyFill() { RelationTableInfo._ReadyFill?.Invoke(); }
    }

    public partial class Table<ValueType, KeyType>
    {
        public class RelationTableInfo<RelationValueType, RelationKeyType> : RelationTableInfo
             where RelationKeyType : IComparable<RelationKeyType>
        {
            public Table<ValueType, KeyType> OwnerArray;
            public Expression<Func<ValueType, PartOfTable<RelationValueType, RelationKeyType>>> Link;
            public Table<RelationValueType, RelationKeyType> LinkArray;
            public DynamicAssembly.FieldControler<ValueType,PartOfTable<RelationValueType,RelationKeyType>> Field;
            internal void Fill<FillerValueType, FillerKeyType>(
                Table<FillerValueType, FillerKeyType> Filler,
                Expression<Func<FillerValueType, PartOfTable<RelationValueType, RelationKeyType>>> SourceLink,
                Expression<Func<FillerValueType, PartOfTable<ValueType, KeyType>>> DestintionLink)
            where FillerKeyType : IComparable<FillerKeyType>
            => RelationTableInfo._ReadyFill += () =>
            {
                
                var Name = "F1" + string.Concat(new string[] {
                    Filler.GetHashCode().ToString(),
                    OwnerArray.GetHashCode().ToString(),
                    LinkArray.GetHashCode().ToString()
                    }.OrderBy((c) => c).ToArray());
                Name = "F1" + Link.Body.ToString();
                var SourceField = DynamicAssembly.FieldControler.Make(SourceLink);
                var DestintionField = DynamicAssembly.FieldControler.Make(DestintionLink);
                var RelationField = DynamicAssembly.FieldControler.Make(Link);
                Filler.Events.loading += (c) =>
                {
                    var Sources = SourceField.Value(c);
                    Sources.Extras.Accepted += (PartOfTable<RelationValueType, RelationKeyType>.TableExtras.KeyInfo ac) =>
                    {
                        if (Run.Use(Name))
                        {
                            Action Ac = null;
                            Ac = () => {
                                Run.OnEndBlocks -= Ac;
                                c = Filler.GetItem(c);
                                foreach (var Des in DestintionField.Value(c))
                                {
                                    var Relation = RelationField.Value(Des);
                                    if (Relation.Parent.KeysInfo.Keys.BinarySearch(ac.Key).Index > -1)
                                        if (Relation.KeysInfo.Keys.BinarySearch(ac.Key).Index < 0)
                                            Relation.Accept(ac.Key);
                                }
                            };
                            Run.OnEndBlocks += Ac;
                        }
                    };
                    Sources.Extras.Ignored += (PartOfTable<RelationValueType, RelationKeyType>.TableExtras.KeyInfo ac) =>
                    {
                        if (Run.Use(Name))
                        {
                            Action Ac = null;
                            Ac = () => {
                                Run.OnEndBlocks -= Ac;
                                c = Filler.GetItem(c);
                                foreach (var Des in DestintionField.Value(c))
                                {
                                    var Relation = RelationField.Value(Des);
                                    if (Relation.KeysInfo.Keys.BinarySearch(ac.Key).Index > -1)
                                        Relation.Ignore(ac.Key);
                                }
                            };
                            Run.OnEndBlocks += Ac;
                        }
                    };

                    var Destintions = DestintionField.Value(c);
                    Destintions.Extras.Accepted += (PartOfTable<ValueType, KeyType>.TableExtras.KeyInfo ac) =>
                    {
                        if (Run.Use(Name))
                        {
                            Action Ac = null;
                            Ac = () => {
                                Run.OnEndBlocks -= Ac;
                                c = Filler.GetItem(c);
                                if (Destintions.KeysInfo.Keys.BinarySearch(ac.Key).Index > -1)
                                {
                                    var Relation = RelationField.Value(Destintions.GetItem(ac.Key));
                                    foreach (var src in SourceField.Value(c).KeysInfo.Keys)
                                    {
                                        if (Relation.Parent.KeysInfo.Keys.BinarySearch(src).Index > -1)
                                            if (Relation.KeysInfo.Keys.BinarySearch(src).Index < 0)
                                                Relation.Accept(src);
                                    }
                                }
                            };
                            Run.OnEndBlocks += Ac;
                        }
                    };
                    Destintions.Extras.Ignored += (PartOfTable<ValueType, KeyType>.TableExtras.KeyInfo ac) =>
                    {
                        if (Run.Use(Name))
                        {
                            Action Ac = null;
                            Ac = () => {
                                Run.OnEndBlocks -= Ac;
                                c = Filler.GetItem(c);
                                if (Destintions.KeysInfo.Keys.BinarySearch(ac.Key).Index > -1)
                                {
                                    var Relation = RelationField.Value(Destintions.GetItem(ac.Key));
                                    foreach (var src in SourceField.Value(c).KeysInfo.Keys)
                                    {
                                        if (Relation.KeysInfo.Keys.BinarySearch(src).Index > -1)
                                            Relation.Ignore(src);
                                    }
                                }
                            };
                            Run.OnEndBlocks += Ac;
                        }
                    };
                };

            };

            internal void Fill<FillerValueType, FillerKeyType>(
                Table<FillerValueType, FillerKeyType> Filler,
                Expression<Func<FillerValueType, PartOfTable<RelationValueType, RelationKeyType>.RelationItem>> SourceLink,
                Expression<Func<FillerValueType, PartOfTable<ValueType, KeyType>>> DestintionLink)
            where FillerKeyType : IComparable<FillerKeyType>
            => RelationTableInfo._ReadyFill += () =>
            {
                var Name = "F1" + string.Concat(new string[] {
                    Filler.GetHashCode().ToString(),
                    OwnerArray.GetHashCode().ToString(),
                    LinkArray.GetHashCode().ToString()
                    }.OrderBy((c) => c).ToArray());
                Name = "F1" + Link.Body.ToString();
                var SourceField = DynamicAssembly.FieldControler.Make(SourceLink);
                var DestintionField = DynamicAssembly.FieldControler.Make(DestintionLink);
                var RelationField = DynamicAssembly.FieldControler.Make(Link);
                Filler.Events.loading += (c) =>
                {
                    var Key = SourceField.Value(c).Key;
                    if (Key != null)
                    {
                        var Destintions = DestintionField.Value(c);
                        Destintions.Extras.Accepted += (PartOfTable<ValueType, KeyType>.TableExtras.KeyInfo ac) =>
                        {
                            if (Run.Use(Name))
                            {
                                Action Ac = null;
                                Ac = () =>
                                {
                                    Run.OnEndBlocks -= Ac;
                                    c = Filler.GetItem(c);
                                    var Relation = RelationField.Value(Destintions.GetItem(ac.Key));
                                    if (Relation.KeysInfo.Keys.BinarySearch((RelationKeyType)Key).Index < 0)
                                        Relation.Accept((RelationKeyType)Key);
                                };
                                Run.OnEndBlocks += Ac;
                            }
                        };
                        Destintions.Extras.Ignored += (PartOfTable<ValueType, KeyType>.TableExtras.KeyInfo ac) =>
                        {
                            if (Run.Use(Name))
                            {
                                Action Ac = null;
                                Ac = () =>
                                {
                                    Run.OnEndBlocks -= Ac;
                                    c = Filler.GetItem(c);
                                    var Relation = RelationField.Value(Destintions.GetItem(ac.Key));
                                    if (Relation.KeysInfo.Keys.BinarySearch((RelationKeyType)Key).Index > -1)
                                        Relation.Ignore((RelationKeyType)Key);
                                };
                                Run.OnEndBlocks += Ac;
                            }
                        };
                    }
                };

                Filler.Events.Saving += (c) =>
                {
                    if (Run.Use(Name))
                    {
                        Action Ac = null;
                        Ac = () =>
                        {
                            Run.OnEndBlocks -= Ac;

                            var Destintions = DestintionField.Value(c);
                            var Source = SourceField.Value(c);
                            if (Source.Key != Source.OldKey)
                            {
                                if (Source.OldKey != null)
                                {
                                    foreach (var Destintion in Destintions)
                                    {
                                        var Relation = RelationField.Value(Destintion);
                                        if (Relation.KeysInfo.Keys.BinarySearch((RelationKeyType)Source.OldKey).Index > -1)
                                            Relation.Ignore((RelationKeyType)Source.OldKey);
                                    }
                                }
                                if (Source.Key != null)
                                {
                                    foreach (var Destintion in Destintions)
                                    {
                                        var Relation = RelationField.Value(Destintion);
                                        if (Relation.KeysInfo.Keys.BinarySearch((RelationKeyType)Source.Key).Index < 0)
                                            Relation.Accept((RelationKeyType)Source.Key);
                                    }
                                }
                            }
                        };
                        Run.OnEndBlocks += Ac;
                    }
                };
            };

            public override string ToString()
            {
                return "RelationTable " +
                    typeof(ValueType).ToString() +" "+
                    typeof(RelationValueType).ToString();
            }
        }

        public RelationTableInfo<RelationValueType, RelationKeyType>
            Relation<RelationValueType, RelationKeyType>(
                Expression<Func<ValueType, PartOfTable<RelationValueType, RelationKeyType>>> RelationLink,
                Action<RelationTableInfo<RelationValueType, RelationKeyType>> Config = null)
            where RelationKeyType : IComparable<RelationKeyType>
        {
            var Configs = new RelationTableInfo<RelationValueType, RelationKeyType>()
            {
                Link = RelationLink,
                OwnerArray = this
            };

            if (Config != null)
            {
                Config.Invoke(Configs);
            }
            else if (DefaultRelationConfigs.DefaultTableInfo != null)
            {
                DefaultRelationConfigs.DefaultTableInfo?.Invoke(Configs);
            }
            return Configs;
        }


        public class RelationItemInfo<RelationValueType, RelationKeyType> : RelationItemInfo
             where RelationKeyType : IComparable<RelationKeyType>
        {
            public Table<ValueType, KeyType> OwnerArray;
            public Expression<Func<ValueType, PartOfTable<RelationValueType, RelationKeyType>.RelationItem>> Link;
            public Table<RelationValueType, RelationKeyType> LinkArray;
            public Func<ValueType, RelationKeyType> AutoFill;
            public DynamicAssembly.FieldControler<ValueType,Table<RelationValueType,RelationKeyType>.RelationItem> Field;
            public override string ToString()
            {
                return "RelationItem " +
                    typeof(ValueType).ToString() +" "+
                    typeof(RelationValueType).ToString();
            }
        }

        public RelationItemInfo<RelationValueType, RelationKeyType>
            Relation<RelationValueType, RelationKeyType>(
                Expression<Func<ValueType, PartOfTable<RelationValueType, RelationKeyType>.RelationItem>> RelationLink,
                Action<RelationItemInfo<RelationValueType, RelationKeyType>> Config = null)
            where RelationKeyType : IComparable<RelationKeyType>
        {
            var Configs = new RelationItemInfo<RelationValueType, RelationKeyType>()
            {
                Link = RelationLink,
                OwnerArray = this
            };

            if (Config != null)
            {
                Config.Invoke(Configs);
            }
            else if (DefaultRelationConfigs.DefaultTableInfo != null)
            {
                DefaultRelationConfigs.DefaultItemInfo?.Invoke(Configs);
            }
            return Configs;
        }
    }
}
