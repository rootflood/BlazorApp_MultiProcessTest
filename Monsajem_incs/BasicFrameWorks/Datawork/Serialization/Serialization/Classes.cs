using Monsajem_Incs.Array.DynamicSize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using static System.Runtime.Serialization.FormatterServices;
using static System.Text.Encoding;

namespace Monsajem_Incs.Serialization
{

    public interface IWhenCanSerialize
    {
        bool CanSerialize { get; }
    }

    public interface IPreSerialize
    {
        void PreSerialize();
    }

    public interface IAfterDeserialize
    {
        void AfterDeserialize();
    }

    public interface ISerializable<DataType>
    {
        DataType GetData();
        void SetData(DataType Data);
    }

    public class NonSerializedAttribute : Attribute
    { }

    public static class SerializationExtentions
    {
        public static readonly Serialization Serializere = new Serialization(
            (c) => c.GetCustomAttributes(typeof(NonSerializedAttribute)).Count() == 0);

        public static byte[] Serialize<t>(this t obj)
        {
            return Serializere.Serialize(obj);
        }

        public static t Deserialize<t>(this byte[] Data)
        {
            return Serializere.Deserialize<t>(Data);
        }

        public static t Deserialize<t>(this byte[] Data, ref int From)
        {
            return Serializere.Deserialize<t>(Data, ref From);
        }

        public static t Deserialize<t>(this byte[] Data, t SampleType)
        {
            return Serializere.Deserialize<t>(Data);
        }
    }
    public partial class Serialization
    {
        public static Serialization Serializere;
        private abstract class SerializeInfo
        {
            public Type Type;
            public int TypeHashCode;
            public Func<object> Deserializer;
            public Action<object> Serializer;
            public byte[] NameAsByte;
            public bool CanStoreInVisit;
            protected bool IsMade;
            private static Type BaseType = typeof(SerializeInfo<object>).GetGenericTypeDefinition();

            public abstract void Make();


            private class ExactSerializer:
                IComparable<ExactSerializer>
            {
                public int HashCode;
                public SerializeInfo Serializer;

                public int CompareTo(ExactSerializer other)
                {
                    return HashCode-other.HashCode;
                }
            }

            private static Array.DynamicSize.SortedArray<ExactSerializer> 
                SerializersByHashCode = new SortedArray<ExactSerializer>(10);
            private static Array.DynamicSize.SortedArray<ExactSerializer> 
                SerializersByNameCode = new SortedArray<ExactSerializer>(10);

            public static SerializeInfo GetSerialize(Type Type)
            {
                SerializeInfo SR;
                var HashCode = Type.GetHashCode();
                var Result = SerializersByHashCode.BinarySearch(
                    new ExactSerializer() { HashCode = HashCode });
                if (Result.Index < 0)
                {
                    SR = (SerializeInfo)
                            BaseType.MakeGenericType(Type).GetMethod("GetSerialize").
                        Invoke(null, null);
                    Result = SerializersByHashCode.BinarySearch(
                        new ExactSerializer() { HashCode = HashCode });
                    if (Result.Index < 0)
                        SerializersByHashCode.BinaryInsert(
                            new ExactSerializer() { HashCode = HashCode, Serializer = SR });
                }
                else
                    SR = Result.Value.Serializer;
#if DEBUG
                if (SR.Type != Type)
                    throw new Exception("invalid Serializers Found!");
#endif
                return SR;
            }

            public static SerializeInfo GetSerialize(string TypeName)
            {
                SerializeInfo SR;
                var HashCode = TypeName.GetHashCode();
                var Result = SerializersByNameCode.BinarySearch(
                                new ExactSerializer() { HashCode = HashCode });
                if (Result.Index < 0)
                {
                    SR = (SerializeInfo)
                            BaseType.MakeGenericType(TypeName.GetTypeByName()).GetMethod("GetSerialize").
                        Invoke(null, null);
                    Result = SerializersByNameCode.BinarySearch(
                        new ExactSerializer() { HashCode = HashCode });
                    if (Result.Index < 0)
                        SerializersByNameCode.BinaryInsert(
                            new ExactSerializer() { HashCode = HashCode, Serializer = SR });
                }
                else
                    SR = Result.Value.Serializer;
#if DEBUG
                if (SR.Type != TypeName.GetTypeByName())
                    throw new Exception("invalid Serializers Found!");
#endif
                return SR;
            }
        }

        private class SerializeInfo<t> : SerializeInfo
        {
            public static Func<object> _Deserializer;
            public static Action<object> _Serializer;

            public SerializeInfo()
            {
                Type = typeof(t);
                TypeHashCode = Type.GetHashCode();

                var TypeName = Type.MidName();
                var Name = UTF8.GetBytes(TypeName);
                NameAsByte = BitConverter.GetBytes(Name.Length);
                Insert(ref NameAsByte, Name);

                CanStoreInVisit =
                    !(Type == typeof(Delegate) |
                      Type.BaseType == typeof(MulticastDelegate) |
                      Type.IsPrimitive);
            }

            private static readonly SerializeInfo<t> SerializerObj = new SerializeInfo<t>();
            public static SerializeInfo<t> GetSerialize()
            {
                var Sr = SerializerObj;
                if (Sr.IsMade == false)
                {
                    Sr.IsMade = true;
                    if (_Serializer == null)
                        Sr.Make();
                    else
                    {
                        Sr.Serializer = _Serializer;
                        Sr.Deserializer = _Deserializer;
                    }
                }
                return Sr;
            }

            public override void Make()
            {
                var Type = typeof(t);

                var IsSerializable = Type.GetInterfaces().Where(
                    (c) =>
                    {
                        if (c.IsGenericType)
                            if (c.GetGenericTypeDefinition() == ISerializableType)
                                return true;
                        return false;
                    }).FirstOrDefault();
                if (IsSerializable != null)
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Serializable();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
                else if (Type == typeof(Delegate))
                {
                    InsertSerializer(
                    (object obj) =>
                    {
                        var DType = obj.GetType();
                        var Serializer = GetSerialize(DType);
                        Serializere.VisitedInfoSerialize<object>(DType.GetHashCode(), () => (Serializer.NameAsByte, null));
                        Serializer.Serializer(obj);
                    },
                    () =>
                    {
                        var Info = Serializere.VisitedInfoDeserialize(() => Serializere.Read());

                        return GetSerialize(Info).Deserializer();
                    });
                }
                else if (Type.GetInterfaces().Where((c) => c == typeof(Array.Base.IArray)).FirstOrDefault() != null)
                {

                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Monsajem_Array();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
                else if (Type.IsInterface)
                {
                    InsertSerializer(() =>
                    {
                        var Sr = SerializeInfo<object>.GetSerialize();
                        return (Sr.Serializer, Sr.Deserializer);
                    });
                }
                else if (Type.IsArray)
                {
                    try
                    {
                        var size = System.Runtime.InteropServices.Marshal.SizeOf(Type.GetElementType());
                        InsertSerializer(() =>
                        {
                            var sr = MakeSerializer_Array_Struct(size);
                            return (sr.Serializer, sr.Deserializer);
                        });
                    }
                    catch
                    {
                        InsertSerializer(() =>
                        {
                            var sr = MakeSerializer_Array_Class();
                            return (sr.Serializer, sr.Deserializer);
                        });
                    }
                }
                else if (Type == typeof(System.Array))
                {
                    InsertSerializer(() =>
                    {
                        var Sr = SerializeInfo<object>.GetSerialize();
                        return (Sr.Serializer, Sr.Deserializer);
                    });
                }
                else if (Type == typeof(System.Array))
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Array_Class();
                        return ((obj) =>
                        {
                            var ar = (System.Array)obj;
                        }, () =>
                        {
                            return null;
                        }
                        );
                    });
                }
                else if (Type.BaseType == typeof(MulticastDelegate))
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Delegate();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
                else if (Nullable.GetUnderlyingType(Type) != null)
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Nullable();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
                else if (Type.IsValueType)
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_ValueType();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
                else
                {
                    InsertSerializer(() =>
                    {
                        var sr = MakeSerializer_Else();
                        return (sr.Serializer, sr.Deserializer);
                    });
                }
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Serializable()
            {
                var Type = typeof(t);
                var InterfaceType = Type.GetInterfaces().Where(
                    (c) => c.GetGenericTypeDefinition() == ISerializableType).
                    FirstOrDefault();

                var InnerType = InterfaceType.GenericTypeArguments[0];
                var innerSerializer = GetSerialize(InnerType);
                var Getter = InterfaceType.GetMethod("GetData");
                var Setter = InterfaceType.GetMethod("SetData");
                Action<object> Serializer = (object obj) =>
                {
                    if (obj == null)
                    {
                        S_Data.Write(Byte_0, 0, 1);
                        return;
                    }

                    S_Data.Write(Byte_1, 0, 1);
                    Serializere.VisitedSerialize(Getter.Invoke(obj, null), innerSerializer);
                };

                Func<object> Deserializer = () =>
                {
                    var ThisFrom = From;
                    if (D_Data[From] == 0)
                    {
                        From += 1;
                        return null;
                    }
                    From += 1;

                    object Result = GetUninitializedObject(Type);
                    Serializere.VisitedDeserialize((c) => Setter.Invoke(Result, new object[] { c }), innerSerializer);
                    return Result;
                };

                return (Serializer, Deserializer);
            }

            private (Func<System.Array, int[]> Write, Func<(int[] Ends, System.Array ar)> Read)
                ArrayGetCreator()
            {
                var Type = typeof(t);
                Func<System.Array, int[]> Write;
                Func<(int[] Ends, System.Array ar)> Read;
                if (Type != typeof(System.Array))
                {
                    var Creator = DynamicAssembly.TypeController.CreateArray(Type);
                    var Rank = Type.GetArrayRank();
                    Write = (ar) =>
                    {
                        var Ends = new int[Rank];
                        for (int i = 0; i < Rank; i++)
                        {
                            Ends[i] = ar.GetUpperBound(i) + 1;
                            S_Data.Write(BitConverter.GetBytes(Ends[i]), 0, 4);
                        }
                        return Ends;
                    };
                    Read = () =>
                    {
                        var Ends = new int[Rank];
                        for (int i = 0; i < Rank; i++)
                        {
                            Ends[i] = BitConverter.ToInt32(D_Data, From);
                            From += 4;
                        }
                        return (Ends, (System.Array)Creator(Ends));
                    };
                }
                else
                {
                    Write = (ar) =>
                    {
                        var Rank = ar.Rank;
                        S_Data.Write(BitConverter.GetBytes(Rank), 0, 4);
                        var Ends = new int[Rank];
                        for (int i = 0; i < Rank; i++)
                        {
                            Ends[i] = ar.GetUpperBound(i) + 1;
                            S_Data.Write(BitConverter.GetBytes(Ends[i]), 0, 4);
                        }
                        return Ends;
                    };
                    Read = () =>
                    {
                        var Rank = BitConverter.ToInt32(D_Data, From);
                        From += 4;
                        var Ends = new int[Rank];
                        for (int i = 0; i < Rank; i++)
                        {
                            Ends[i] = BitConverter.ToInt32(D_Data, From);
                            From += 4;
                        }
                        return (Ends, System.Array.CreateInstance(typeof(string), Ends));
                    };
                }
                return (Write, Read);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Array_Class()
            {
                var Type = typeof(t);
                var SR = ArrayMakeSerializer_Object(Type);
                var Creator = ArrayGetCreator();
                Action<object> Serializer = (object obj) =>
                {
                    var ar = (System.Array)obj;
                    SR.Serializer((ar, Creator.Write(ar)));
                };
                Func<object> Deserializer = () =>
                {
                    var info = Creator.Read();
                    SR.Deserializer((info.ar, info.Ends));
                    return info.ar;
                };
                return (Serializer, Deserializer);
            }

            private (Action<(System.Array ar, int[] Ends)> Serializer,
                     Action<(System.Array ar, int[] Ends)> Deserializer)
                ArrayMakeSerializer_Object(Type Type)
            {
                var Setter = DynamicAssembly.TypeController.SetArray(Type);
                var Getter = DynamicAssembly.TypeController.GetArray(Type);
                var ElementType = Type.GetElementType();
                var ItemsSerializer = GetSerialize(Type.GetElementType());

                Action<(System.Array ar, int[] Ends)> Serializer = (obj) =>
                {
                    var ar = obj.ar;
                    var Ends = obj.Ends;
                    var Rank = Ends.Length;
                    var Currents = new int[Rank];
                    while (Currents[Currents.Length - 1] < Ends[Ends.Length - 1])
                    {
                        for (Currents[0] = 0; Currents[0] < Ends[0]; Currents[0]++)
                        {
                            Serializere.VisitedSerialize(Getter(ar, Currents), ItemsSerializer);
                        }
                        for (int i = 1; i < Rank; i++)
                        {
                            if (Currents[i] < Ends[i])
                            {
                                Currents[i]++;
                                Currents[i - 1] = 0;
                            }
                        }
                    }
                };
                Action<(System.Array ar, int[] Ends)> Deserializer = (obj) =>
                {
                    var ar = obj.ar;
                    var Ends = obj.Ends;
                    var Rank = Ends.Length;
                    var Currents = new int[Rank];

                    while (Currents[Currents.Length - 1] < Ends[Ends.Length - 1])
                    {
                        for (Currents[0] = 0; Currents[0] < Ends[0]; Currents[0]++)
                        {
                            var StandAloneCurrent = new int[Rank];
                            for (int i = 0; i < Rank; i++)
                                StandAloneCurrent[i] = Currents[i];
                            Serializere.VisitedDeserialize((c) => Setter(ar, c, StandAloneCurrent), ItemsSerializer);
                        }
                        for (int i = 1; i < Rank; i++)
                        {
                            if (Currents[i] < Ends[i])
                            {
                                Currents[i]++;
                                Currents[i - 1] = 0;
                            }
                        }
                    }
                };
                return (Serializer, Deserializer);
            }
            private unsafe (Action<System.Array> Serializer,
             Action<(System.Array ar, int[] Ends)> Deserializer)
                ArrayMakeSerializer_Struct(Type Type, int size)
            {

                if (Type.GetElementType() == typeof(bool))
                    size = 1;

                Action<System.Array> Serializer = (ar) =>
                {
                    byte[] bytes = new byte[ar.Length * size];

                    System.Runtime.InteropServices.GCHandle h =
                        System.Runtime.InteropServices.GCHandle.Alloc(ar,
                            System.Runtime.InteropServices.GCHandleType.Pinned);

                    var ptr = h.AddrOfPinnedObject();
                    h.Free();
                    System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, bytes.Length);


                    S_Data.Write(bytes, 0, bytes.Length);
                };
                Action<(System.Array ar, int[] Ends)> Deserializer = (obj) =>
                {
                    var ar = obj.ar;
                    var Ends = obj.Ends;
                    var Rank = Ends.Length;
                    var Len = 0;
                    Len = Ends[0];
                    for (int i = 1; i < Rank; i++)
                    {
                        Len = Len * Ends[i];
                    }
                    Len = Len * size;

                    System.Runtime.InteropServices.GCHandle h =
                        System.Runtime.InteropServices.GCHandle.Alloc(ar,
                            System.Runtime.InteropServices.GCHandleType.Pinned);

                    var ptr = h.AddrOfPinnedObject();
                    h.Free();
                    System.Runtime.InteropServices.Marshal.Copy(D_Data, From, ptr, Len);
                    From += Len;
                };
                return (Serializer, Deserializer);
            }
            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Array_Struct(int size)
            {
                var Type = typeof(t);
                var Sr = ArrayMakeSerializer_Struct(Type, size);
                var Creator = ArrayGetCreator();

                Action<object> Serializer = (object obj) =>
                {
                    var ar = (System.Array)obj;
                    Creator.Write(ar);
                    Sr.Serializer(ar);
                };
                Func<object> Deserializer = () =>
                {
                    var info = Creator.Read();
                    Sr.Deserializer((info.ar, info.Ends));
                    return info.ar;
                };
                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
            MakeSerializer_Monsajem_Array()
            {
                var Type = typeof(t);
                var ItemsSerializer = GetSerialize(System.Array.CreateInstance(
                    ((Array.Base.IArray)GetUninitializedObject(Type)).ElementType, 0).GetType());
                var ObjSerializer = SerializeInfo<object>.GetSerialize();

                Action<object> Serializer = (object obj) =>
                {
                    var ar = (Array.Base.IArray)obj;
                    ObjSerializer.Serializer(ar.MyOptions);
                    var AllArrays = ar.GetAllArrays();
                    var Arrays = AllArrays.Ar;
                    var ArraysLen = Arrays.Length;
                    S_Data.Write(BitConverter.GetBytes(ArraysLen), 0, 4);
                    for (int i = 0; i < ArraysLen; i++)
                    {
                        var Array = Arrays[i];
                        S_Data.Write(BitConverter.GetBytes(Array.From), 0, 4);
                        S_Data.Write(BitConverter.GetBytes(Array.To), 0, 4);
                        ItemsSerializer.Serializer(Array.Ar);
                    }
                };
                Func<object> Deserializer = () =>
                {
                    var ar = (Array.Base.IArray)GetUninitializedObject(Type);
                    ar.MyOptions = ObjSerializer.Deserializer();
                    var ArraysLen = BitConverter.ToInt32(D_Data, From);
                    From += 4;
                    var AllArrays = (Ar: new (int From, int To, System.Array Ar)[ArraysLen], MaxLen: 0);
                    var AllLen = 0;
                    for (int i = 0; i < ArraysLen; i++)
                    {
                        var ArFrom = BitConverter.ToInt32(D_Data, From);
                        From += 4;
                        var ArLen = BitConverter.ToInt32(D_Data, From);
                        From += 4;
                        var Array = ItemsSerializer.Deserializer();
                        AllLen += ArLen;
                        AllArrays.Ar[i] = (ArFrom, ArLen, (System.Array)Array);
                    }
                    ar.SetAllArrays(AllArrays);
                    ar.SetLen(AllLen);
                    return ar;
                };
                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Delegate()
            {
                var Type = typeof(t);
                Action<object> Serializer = (object obj) =>
                {
                    var MD = (MulticastDelegate)obj;
                    var Delegates = MD.GetInvocationList();
                    S_Data.Write(BitConverter.GetBytes(Delegates.Length), 0, 4);
                    for (int i = 0; i < Delegates.Length; i++)
                    {
                        LoadedFunc LoadedFunc;
                        var Delegate = Delegates[i];
                        var HashCode = Delegate.Method.GetHashCode();
                        LoadedFunc = Serializere.VisitedInfoSerialize(HashCode,
                        () =>
                        {
                            var FuncPos = System.Array.BinarySearch(Serializere.LoadedFuncs_Ser,
                            new LoadedFunc()
                            {
                                Hash = HashCode
                            });
                            if (FuncPos < 0)
                            {
                                var TargetType = Delegate.Method.DeclaringType;

                                LoadedFunc = new LoadedFunc()
                                {
                                    Hash = HashCode,
                                    NameAsByte = Serializere.Write(
                                        Delegate.Method.Name,
                                        Delegate.Method.ReflectedType.MidName()),
                                    SerializerTarget = GetSerialize(TargetType)
                                };
                                BinaryInsert(ref Serializere.LoadedFuncs_Ser, LoadedFunc);
                            }
                            else
                            {
                                LoadedFunc = Serializere.LoadedFuncs_Ser[FuncPos];
                            }
                            return (LoadedFunc.NameAsByte, LoadedFunc);
                        });
                        var Target = Delegates[i].Target;
                        Serializere.VisitedSerialize(Target, LoadedFunc.SerializerTarget);
                    }
                };

                Func<object> Deserializer = () =>
                {
                    var Count = BitConverter.ToInt32(D_Data, From);
                    From += 4;
                    var Results = new Delegate[Count];
                    for (int i = 0; i < Count; i++)
                    {
                        LoadedFunc LoadedFunc;
                        LoadedFunc = Serializere.VisitedInfoDeserialize(() =>
                        {
                            var MethodName = Serializere.Read();
                            var TypeName = Serializere.Read();

                            var FuncPos = System.Array.BinarySearch(Serializere.LoadedFuncs_Des, new LoadedFunc() { Hash = (MethodName + TypeName).GetHashCode() });
                            if (FuncPos < 0)
                            {
                                var ReflectedType = TypeName.GetTypeByName();
                                var Method = ReflectedType.GetMethod(MethodName,
                                    BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.CreateInstance |
                                    BindingFlags.Instance);

                                var TargetType = Method.DeclaringType;
                                LoadedFunc = new LoadedFunc()
                                {
                                    Delegate = Method.CreateDelegate(Type, null),
                                    Hash = (MethodName + TypeName).GetHashCode(),
                                    SerializerTarget = GetSerialize(TargetType)
                                };
                                BinaryInsert(ref Serializere.LoadedFuncs_Des, LoadedFunc);
                            }
                            else
                            {
                                LoadedFunc = Serializere.LoadedFuncs_Des[FuncPos];
                            }
                            return LoadedFunc;
                        });

                        var ThisDelegate = (Delegate)LoadedFunc.Delegate.Clone();
                        Results[i] = ThisDelegate;
                        Serializere.VisitedDeserialize((c) => Deletage_Target.SetValue(ThisDelegate, c), LoadedFunc.SerializerTarget);
                    }

                    var Result = Delegate.Combine(Results);
                    return Result;
                };

                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Nullable()
            {
                var Type = typeof(t);
                var InnerType = Nullable.GetUnderlyingType(Type);
                var innerSerializer = GetSerialize(InnerType);

                Action<object> Serializer = (object obj) =>
                {
                    innerSerializer.Serializer(obj);
                };

                var CreateInstance = Type.GetConstructor(new Type[] { InnerType });

                Func<object> Deserializer = () =>
                {
                    object Result = innerSerializer.Deserializer();
                    return CreateInstance.Invoke(new object[] { Result });
                };

                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_ValueType()
            {
                var Type = typeof(t);
                var FieldsSerializer = MakeFieldsSerializer(Type);

                Action<object> Serializer = FieldsSerializer.Serializer;

                Func<object> Deserializer = FieldsSerializer.Deserializer;

                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeSerializer_Else()
            {
                var Type = typeof(t);
                var FieldsSerializer = MakeFieldsSerializer(Type);

                Action<object> Serializer = FieldsSerializer.Serializer;

                if (Type.GetInterfaces().Where((c) => c == typeof(IPreSerialize)).Count() > 0)
                {
                    var BaseSerializer = Serializer;
                    Serializer = (object obj) =>
                    {
                        ((IPreSerialize)obj).PreSerialize();
                        BaseSerializer(obj);
                    };
                }

                if (Type.GetInterfaces().Where((c) => c == typeof(IWhenCanSerialize)).Count() > 0)
                {
                    var BaseSerializer = Serializer;
                    Serializer = (object obj) =>
                    {
                        if (((IWhenCanSerialize)obj).CanSerialize == false)
                        {
                            S_Data.Write(Byte_0, 0, 1);
                        }
                        else
                        {
                            BaseSerializer(obj);
                        }
                    };
                }

                Func<object> Deserializer = FieldsSerializer.Deserializer;

                if (Type.GetInterfaces().Where((c) => c == typeof(IAfterDeserialize)).Count() > 0)
                {
                    var BaseDeserializer = Deserializer;
                    Deserializer = () =>
                    {
                        var Result = BaseDeserializer();
                        ((IAfterDeserialize)Result).AfterDeserialize();
                        return Result;
                    };
                }

                return (Serializer, Deserializer);
            }

            private (Action<object> Serializer, Func<object> Deserializer)
                MakeFieldsSerializer(
                Type Type)
            {
                var Filds = new DynamicAssembly.TypeFields(Type).Fields;
                Filds = Filds.Where((c) => Serializere.FieldCondition(c.Info)).ToArray();

                Filds = Filds.OrderBy((c) => c.Info.Name + c.Info.DeclaringType.FullName,
                                      StringComparer.Ordinal).ToArray();

                var FildSerializer = new (SerializeInfo Sr,
                                          DynamicAssembly.FieldControler Controller)[Filds.Length];

                var FildsLen = Filds.Length;
                for (int i = 0; i < FildsLen; i++)
                {
                    var ExactSerializer = GetSerialize(Filds[i].Info.FieldType);
                    FildSerializer[i] = (ExactSerializer, Filds[i]);
                }

                Action<object> Serializer = (object obj) =>
                {
#if DEBUG
                    S_Data.Write(BitConverter.GetBytes(FildsLen), 0, 4);
                    for (int i = 0; i < FildsLen; i++)
                    {
                        var Field = FildSerializer[i];
                        var FieldName = Serializere.Write(Field.Controller.Info.DeclaringType.ToString() + "." + Field.Controller.Info.Name);
                        S_Data.Write(FieldName, 0, FieldName.Length);
                    }
#endif
                    for (int i = 0; i < FildsLen; i++)
                    {
                        var Field = FildSerializer[i];
                        Serializere.VisitedSerialize(Field.Controller.GetValue(obj), Field.Sr);
                    }
                };

                Func<object> Deserializer = () =>
                {
                    object Owner = GetUninitializedObject(Type);
#if DEBUG
                    var F_len = BitConverter.ToInt32(D_Data, From);
                    From += 4;
                    string[] Fields_Types = new string[F_len];
                    for (int i = 0; i < Fields_Types.Length; i++)
                        Fields_Types[i] = Serializere.Read();
                    for (int i = 0; i < Fields_Types.Length; i++)
                    {
                        var Field = FildSerializer[i];
                        var FieldName = Fields_Types[i];
                        if (FieldName != Field.Controller.Info.DeclaringType.ToString() + "." + Field.Controller.Info.Name)
                        {
                            var EX_Str = "Wrong place of fields ";
                            for (int j = 0; j < Fields_Types.Length; j++)
                            {
                                FieldName = Fields_Types[j];
                                EX_Str += "\n SR:" + FieldName;
                            }
                            EX_Str += "\n";
                            for (int j = 0; j < FildSerializer.Length; j++)
                            {
                                Field = FildSerializer[j];
                                EX_Str += "\n DR:" + Field.Controller.Info.DeclaringType.ToString() + "." + Field.Controller.Info.Name;
                            }
                            throw new Exception(EX_Str);
                        }
                    }
#endif
                    for (int i = 0; i < FildsLen; i++)
                    {
                        var Field = FildSerializer[i];
                        Serializere.VisitedDeserialize((c) => Field.Controller.SetValue(Owner, c), Field.Sr);
                    }
                    return Owner;
                };
                return (Serializer, Deserializer);
            }

            public static SerializeInfo<t> InsertSerializer(
                Action<object> Serializer,
                Func<object> Deserializer)
            {
                _Serializer = Serializer;
                _Deserializer = Deserializer;
                return InsertSerializer(() => (Serializer, Deserializer));
            }
            private static SerializeInfo<t> InsertSerializer(
                Func<(Action<object> Sr, Func<object> Dr)> Serializer)
            {
                var Serialize = GetSerialize();
                var Sr = Serializer();

#if DEBUG
                var SR = Sr.Sr;
                var DR = Sr.Dr;
                Sr.Sr = (obj) =>
                {
                    var Pos = S_Data.Position;
                    Tracer($"Type: {Serialize.Type.ToString()} Pos:({Pos})");
                    Check_SR();
                    SR(obj);
                    UnTracer($"Type: {Serialize.Type.ToString()} Pos:({Pos})");
                };
                Sr.Dr = () =>
                {
                    var Pos = From;
                    Tracer($"Type: {Serialize.Type.ToString()} Pos:({Pos})");
                    Check_DR();
                    var Result = DR();
                    UnTracer($"Type: {Serialize.Type.ToString()} Pos:({Pos})");
                    return Result;
                };
#endif

                Serialize.Serializer = Sr.Sr;
                Serialize.Deserializer = Sr.Dr;
                return Serialize;
            }

#if DEBUG
            private static void Check_SR()
            {
                var Type = typeof(t);
                S_Data.Write(BitConverter.GetBytes(S_Data.Length), 0, 8);
                var TypeBytes = Serializere.Write(Type.MidName());
                S_Data.Write(TypeBytes, 0, TypeBytes.Length);
            }
            private static void Check_DR()
            {
                var Type = typeof(t);
                var DR_Pos = From;
                var SR_Pos = BitConverter.ToInt64(D_Data, From);
                From += 8;
                if (DR_Pos != SR_Pos ||
                    SR_Pos < 0)
                    throw new Exception($"Position Isn't Valid. SR:{SR_Pos} , DR:{DR_Pos}");
                var TypeName = Serializere.Read();
                var SR_Type = TypeName.GetTypeByName();
                if (SR_Type != Type)
                    throw new Exception($"Type isn't match\nSR: {SR_Type.MidName()}\nDR: {Type.MidName()}");
            }

            private static void Tracer(string On)
            {
                Traced += "\n >> " + On;
            }
            private static void UnTracer(string On)
            {
                Traced = Traced.Substring(0, Traced.Length - (On.Length + "\n >> ".Length));
            }

#endif

        }

        private class LoadedFunc :
            IComparable<LoadedFunc>
        {
            public int Hash;
            public Delegate Delegate;
            public byte[] NameAsByte;
            public SerializeInfo SerializerTarget;
            public int CompareTo(LoadedFunc other)
            {
                return this.Hash - other.Hash;
            }
        }
        private static Type ISerializableType = typeof(ISerializable<object>).GetGenericTypeDefinition();
        private static byte[] Byte_0 = new byte[] { 0 };
        private static byte[] Byte_1 = new byte[] { 1 };
        private static byte[] Byte_Int_N_1 = BitConverter.GetBytes(-1);
        private static byte[] Byte_Int_N_2 = BitConverter.GetBytes(-2);
        internal static FieldInfo Deletage_Target = ((Func<FieldInfo>)(() =>
        {
            return DynamicAssembly.FieldControler.GetFields(typeof(Delegate)).
                        Where((c) => c.Name.ToLower().Contains("target")).FirstOrDefault();
        }))();

        public Serialization(Func<FieldInfo, bool> FieldCondition) :
            this()
        {
            this._FieldCondition = FieldCondition;
        }

        private Func<FieldInfo, bool> _FieldCondition;
        private bool FieldCondition(FieldInfo Field)
        {
            if (_FieldCondition?.Invoke(Field) == false)
                return false;
            if (Field.DeclaringType.IsGenericType)
                if (Field.DeclaringType.GetGenericTypeDefinition() ==
                    typeof(Dictionary<string, string>).GetGenericTypeDefinition() &&
                    Field.Name == "_syncRoot")
                    return false;
            return true;
        }

        public Serialization()
        {
            Serializere = this;
            SerializeInfo<object>.InsertSerializer(
            (object obj) =>
            {
                var Type = obj.GetType();
                if (Type == typeof(object))
                {
                    S_Data.Write(Byte_0, 0, 1);
                    return;
                }
                var Serializer = SerializeInfo.GetSerialize(Type);

                S_Data.Write(Byte_1, 0, 1);
                VisitedInfoSerialize<object>(Type.GetHashCode(), () => (Serializer.NameAsByte, null));
                Serializer.Serializer(obj);
            },
            () =>
            {
                if (D_Data[From] == 0)
                {
                    From += 1;
                    return new object();
                }
                From += 1;
                var Info = VisitedInfoDeserialize(() =>
                {
                    return Read();
                });

                return SerializeInfo.GetSerialize(Info).Deserializer();
            });

            SerializeInfo<bool>.InsertSerializer(
            (object obj) =>
            {
                if ((bool)obj == true)
                    S_Data.Write(Byte_1, 0, 1);
                else
                    S_Data.Write(Byte_0, 0, 1);
            },
            () =>
            {
                int Position = From; From += 1;
                var Result = (D_Data)[Position];
                return Result > 0;
            });

            SerializeInfo<char>.InsertSerializer(
            (object Obj) =>
            {
                S_Data.Write(BitConverter.GetBytes((char)Obj), 0, 2);
            },
            () =>
            {
                int Position = From; From += 2;
                return BitConverter.ToChar(D_Data, Position);
            });

            SerializeInfo<byte>.InsertSerializer(
            (object Obj) =>
            {
                S_Data.WriteByte((byte)Obj);
            },
            () =>
            {
                int Position = From; From += 1;
                return D_Data[Position];
            });

            SerializeInfo<sbyte>.InsertSerializer(
            (object Obj) =>
            {
                S_Data.WriteByte((byte)((sbyte)Obj));
            },
            () =>
            {
                int Position = From; From += 1;
                return (sbyte)D_Data[Position];
            });

            SerializeInfo<short>.InsertSerializer(
            (object obj) =>
            {
                S_Data.Write(BitConverter.GetBytes((short)obj), 0, 2);
            },
            () =>
            {
                int Position = From; From += 2;
                return BitConverter.ToInt16(D_Data, Position);
            });

            SerializeInfo<ushort>.InsertSerializer(
            (object Obj) =>
            {
                S_Data.Write(BitConverter.GetBytes((ushort)Obj), 0, 2);
            },
            () =>
            {                 /// as UInt16
                int Position = From; From += 2;
                return BitConverter.ToUInt16(D_Data, Position);
            });

            SerializeInfo<int>.InsertSerializer(
            (object obj) =>
            {                 /// as Int32
                S_Data.Write(BitConverter.GetBytes((int)obj), 0, 4);
            },
            () =>
            {                 /// as Int32
                int Position = From; From += 4;
                return BitConverter.ToInt32(D_Data, Position);
            });

            SerializeInfo<uint>.InsertSerializer(
            (object obj) =>
            {                 /// as UInt32
                S_Data.Write(BitConverter.GetBytes((uint)obj), 0, 4);
            },
            () =>
            {                 /// as UInt32
                int Position = From; From += 4;
                return BitConverter.ToUInt32(D_Data, Position);
            });

            SerializeInfo<long>.InsertSerializer(
            (object obj) =>
            {                 /// as Int64
                S_Data.Write(BitConverter.GetBytes((long)obj), 0, 8);
            },
            () =>
            {                 /// as Int64
                int Position = From; From += 8;
                return BitConverter.ToInt64(D_Data, Position);
            });

            SerializeInfo<ulong>.InsertSerializer(
            (object obj) =>
            {
                S_Data.Write(BitConverter.GetBytes((ulong)obj), 0, 8);
            },
            () =>
            {
                int Position = From; From += 8;
                return BitConverter.ToUInt64(D_Data, Position);
            });

            SerializeInfo<float>.InsertSerializer(
            (object obj) =>
            {    /// as float
                S_Data.Write(BitConverter.GetBytes((float)obj), 0, 4);
            },
            () =>
            {    /// as float
                int Position = From; From += 4;
                return BitConverter.ToSingle(D_Data, Position);
            });

            SerializeInfo<double>.InsertSerializer(
            (object obj) =>
            {                 /// as double
                S_Data.Write(BitConverter.GetBytes((double)obj), 0, 8);
            },
            () =>
            {                 /// as double
                int Position = From; From += 8;
                return BitConverter.ToDouble(D_Data, Position);
            });

            SerializeInfo<DateTime>.InsertSerializer(
            (object obj) =>
            {
                S_Data.Write(BitConverter.GetBytes(((DateTime)obj).Ticks), 0, 8);
            },
            () =>
            {
                int Position = From; From += 8;
                return DateTime.FromBinary(BitConverter.ToInt64(D_Data, Position));
            });

            SerializeInfo<string>.InsertSerializer(
            (object obj) =>
            {
                if (obj == null)
                {
                    S_Data.Write(BitConverter.GetBytes(-1), 0, 4);
                    return;
                }
                var Str = UTF8.GetBytes((string)obj);
                var Len = BitConverter.GetBytes(Str.Length);
                S_Data.Write(Len, 0, 4);
                S_Data.Write(Str, 0, Str.Length);
            },
            () =>
            {
                var StrSize = BitConverter.ToInt32(D_Data, From);
                From += 4;
                if (StrSize == -1)
                    return null;
                var Position = From;
                From += StrSize;
                return UTF8.GetString(D_Data, Position, StrSize);
            });

            SerializeInfo<IntPtr>.InsertSerializer(
            (object obj) =>
            {                 /// as IntPtr
                S_Data.Write(BitConverter.GetBytes(((IntPtr)obj).ToInt64()), 0, 8);
            },
            () =>
            {                 /// as IntPtr
                int Position = From; From += 8;
                return new IntPtr(BitConverter.ToInt64(D_Data, Position));
            });

            SerializeInfo<UIntPtr>.InsertSerializer(
            (object obj) =>
            {                 /// as UIntPtr
                S_Data.Write(BitConverter.GetBytes(((UIntPtr)obj).ToUInt64()), 0, 8);
            },
            () =>
            {                 /// as UIntPtr
                int Position = From; From += 8;
                return new UIntPtr(BitConverter.ToUInt64(D_Data, Position));
            });

            SerializeInfo<decimal>.InsertSerializer(
            (object obj) =>
            {                 /// as Decimal
                S_Data.Write(BitConverter.GetBytes(Decimal.ToDouble((decimal)obj)), 0, 8);
            },
            () =>
            {                 /// as Decimal
                int Position = From; From += 8;
                return System.Convert.ToDecimal(BitConverter.ToDouble(D_Data, Position));
            });

            SerializeInfo<Type>.InsertSerializer(
                   (object obj) =>
                   {
                       var Name = Write(((Type)obj).MidName());
                       S_Data.Write(Name, 0, Name.Length);
                   },
                   () =>
                   {
                       return Read().GetTypeByName();
                   });

            {
                var SR = SerializeInfo<object>.GetSerialize();
                SerializeInfo<System.Runtime.InteropServices.GCHandle>.InsertSerializer(
                    (object obj) =>
                    {
                        var GC = (System.Runtime.InteropServices.GCHandle)obj;
                        SR.Serializer(GC.Target);
                    },
                    () =>
                    {
                        return System.Runtime.InteropServices.GCHandle.Alloc(SR.Deserializer());
                    });
            }

            SerializeInfo<IEqualityComparer<string>>.InsertSerializer(
                (object obj) =>
                {
                    if (obj == null)
                        S_Data.Write(Byte_0, 0, 1);
                    else
                        S_Data.Write(Byte_1, 0, 1);
                },
                () =>
                {
                    if (D_Data[From++] == 0)
                        return null;
                    else
                        return EqualityComparer<string>.Default;
                });
        }

        private LoadedFunc[] LoadedFuncs_Des = new LoadedFunc[0];
        private LoadedFunc[] LoadedFuncs_Ser = new LoadedFunc[0];

        [ThreadStaticAttribute]
        private static byte[] D_Data;
        [ThreadStaticAttribute]
        private static int From;

        [ThreadStaticAttribute]
        private static MemoryStream S_Data;
        [ThreadStaticAttribute]
        private static SortedArray<ObjectContainer> Visitor;
        [ThreadStaticAttribute]
        private static SortedArray<ObjectContainer> Visitor_info;

        public byte[] Serialize<t>(t obj)
        {
#if DEBUG
            var Result = _Serialize(obj);
            var DS = Deserialize<t>(Result);
            return Result;
#else
            return _Serialize(obj);
#endif
        }

        private byte[] _Serialize<t>(t obj)
        {
            lock (this)
            {
                byte[] Result;

                if (Serialization.S_Data == null)
                {
                    Serialization.S_Data = new MemoryStream();
                    Serialization.Visitor = new SortedArray<ObjectContainer>(20);
                    Serialization.Visitor_info = new SortedArray<ObjectContainer>(20);
                }
                var SR = SerializeInfo<t>.GetSerialize();
                try
                {
#if DEBUG
                    if (Deletage_Target == null)
                    {
                        var Fields = DynamicAssembly.FieldControler.GetFields(typeof(Delegate));
                        var Fields_str = "";
                        for (int i = 0; i < Fields.Length; i++)
                        {
                            Fields_str += "\n" + Fields[i].Name;
                        }
                        throw new Exception("Cant Access to Deletage Target field at serializer, Fields >>" + Fields_str);
                    }
#endif
                    VisitedSerialize(obj, SR);
                    Result = S_Data.ToArray();
                }
#if DEBUG
                catch (Exception ex)
                {
                    var Traced = Serialization.Traced;
                    if (Traced != null)
                        Traced = "On " + Traced;
                    Serialization.Traced = null;
                    throw new Exception($"Serialize Of Type >> {obj.GetType().FullName} Is Failed " + Traced, ex);
                }
#endif
                finally
                {
                    Serialization.S_Data.SetLength(0);
                    Serialization.Visitor.Clear();
                    Serialization.Visitor_info.Clear();
                }
                return Result;
            }
        }

#if DEBUG
        [ThreadStaticAttribute]
        private static string Traced;
#endif

        public t Deserialize<t>(byte[] Data)
        {
            var Type = typeof(t);
            var From = 0;
            return Deserialize<t>(Data, ref From);
        }

        public t Deserialize<t>(byte[] Data, ref int From)
        {
            lock (this)
            {
                t Result = default;
                if (Serialization.Visitor_info == null)
                {
                    Serialization.Visitor = new SortedArray<ObjectContainer>(20);
                    Serialization.Visitor_info = new SortedArray<ObjectContainer>(20);
                }
                Serialization.D_Data = Data;
                Serialization.From = From;
                try
                {
#if DEBUG
                    if (Deletage_Target == null)
                    {
                        var Fields = DynamicAssembly.FieldControler.GetFields(typeof(Delegate));
                        var Fields_str = "";
                        for (int i = 0; i < Fields.Length; i++)
                        {
                            Fields_str += "\n" + Fields[i].Name;
                        }
                        throw new Exception("Cant Access to Deletage Target field at serializer, Fields >>" + Fields_str);
                    }
#endif
                    VisitedDeserialize((c) => Result = (t)c, SerializeInfo<t>.GetSerialize());
                    AtLast?.Invoke();
                }
#if DEBUG
                catch (Exception ex)
                {
                    var Traced = Serialization.Traced;
                    if (Traced != null)
                        Traced = "On " + Traced;
                    Serialization.Traced = null;
                    throw new Exception($"Deserialize From Point {Serialization.From} Of Type >> {typeof(t).FullName} Is Failed {Traced}\nDatas As B64:\n" + System.Convert.ToBase64String(Data), ex);
                }
#endif
                finally
                {
                    Serialization.Visitor.Clear();
                    Serialization.Visitor_info.Clear();
                    Serialization.D_Data = null;
                    AtLast = null;
                }
                return Result;
            }
        }
    }
}