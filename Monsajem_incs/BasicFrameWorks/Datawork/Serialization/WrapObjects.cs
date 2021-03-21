using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using Monsajem_Incs.ArrayExtentions;
using static System.Runtime.Serialization.FormatterServices;
using Monsajem_Incs.Array.DynamicSize;

namespace Monsajem_Incs.Serialization
{
    public static class WrapObjectExtensions
    {
        [ThreadStatic]
        private static SortedArray<ObjectContainer> visited;

        public static ((Action<object> Set, Func<object> Get)[] Wraps, Func<object> Root) Wrap(this object originalObject)
        {
            if (originalObject == null)
                return (new (Action<object> Set, Func<object> Get)[0], () => null);
            var Pos = InternalCopy(originalObject.GetType());
            visited = new SortedArray<ObjectContainer>(5);
            InternalCopys[Pos](originalObject);
            Clone((obj) => originalObject = obj,
                  () => originalObject);
            return (visited.Select((c) => (c.Set, c.Get)).ToArray(),()=>originalObject);
        }

        private static void Clone(
            Action<object> Set,
            Func<object> Get,
            Action<object> Copy=null)
        {
#if DEBUG
            if (Set == null)
                throw new Exception("Set Not implemented");
            if (Get == null)
                throw new Exception("Get Not implemented");
#endif
            object originalObject = Get();
            if (originalObject == null)
                return;
            var VisitedObj = new ObjectContainer()
            {
                ObjHashCode = originalObject.GetHashCode(),
                TypeHashCode = originalObject.GetType().GetHashCode()
            };
            var VisitedPos = visited.BinarySearch(VisitedObj);
            if (VisitedPos.Index < 0)
            {
                visited.BinaryInsert(VisitedObj);
                VisitedObj.obj = originalObject;
                VisitedObj.Set = Set;
                VisitedObj.Get = Get;
                Copy?.Invoke(originalObject);
            }
            else
            {
                VisitedObj = VisitedPos.Value;
                VisitedObj.Set += Set;
            }
        }

        private static int InternalCopy(Type typeToReflect)
        {
            var pos = System.Array.BinarySearch(TypeCodes, typeToReflect.GetHashCode());
            if (pos > -1)
            {
                return CopyPoss[pos];
            }
            else
            {
                pos = InternalCopys.Length;
                Insert(ref InternalCopys, (Action<object>)null);
                Insert(ref CopyPoss, pos, BinaryInsert(ref TypeCodes, typeToReflect.GetHashCode()));
                Action<object> MyInternalCopy;

                if (Type.GetTypeCode(typeToReflect) != TypeCode.Object||
                    typeof(IntPtr) == typeToReflect ||
                    typeof(UIntPtr) == typeToReflect)
                {
                    MyInternalCopy = (c) => { };
                }
                else if (typeToReflect.IsArray | typeToReflect == typeof(System.Array))
                {
                    Func<object, (Type ElementType, int Rank, Action<object> Copy)> GetInfo = null;
                    Action<object>
                        MyInternalCopy_ArrDelegate = (originalObject) => { };
                    Action<object> MyInternalCopy_Arr;
                    {
                        MyInternalCopy_Arr = (originalObject) =>
                        {
                            var Info = GetInfo(originalObject);

                            var ArrayObject = (System.Array)originalObject;
                            var lents = new int[Info.Rank];
                            for (int i = 0; i < Info.Rank; i++)
                            {
                                lents[i] = ArrayObject.GetUpperBound(i) + 1;
                            }

                            ArrayObject.ForEach(lents, (indices) =>
                            {
                                var StandAloneCurrent = new int[Info.Rank];
                                for (int i = 0; i < Info.Rank; i++)
                                    StandAloneCurrent[i] = indices[i];
                                Clone((obj) => ArrayObject.SetValue(obj, StandAloneCurrent),
                                      () => ArrayObject.GetValue(StandAloneCurrent), Info.Copy);
                            });
                        };
                    }

                    if (typeToReflect == typeof(System.Array))
                    {
                        MyInternalCopy = (originalObject) =>
                        {
                            var Pos = InternalCopy(originalObject.GetType());
                            InternalCopys[Pos](originalObject);
                        };
                    }
                    else
                    {
                        var ElementType = typeToReflect.GetElementType();
                        var Rank = typeToReflect.GetArrayRank();
                        var ICPos = InternalCopy(typeToReflect.GetElementType());
                        var ArrayInternalCopy = InternalCopys[ICPos];
                        GetInfo = (ar) => (ElementType, Rank, ArrayInternalCopy);
                        if (typeof(Delegate).IsAssignableFrom(typeToReflect))
                        {
                            MyInternalCopy = MyInternalCopy_ArrDelegate;
                        }
                        else
                        {
                            MyInternalCopy = MyInternalCopy_Arr;
                        }
                    }
                }
                else
                {
                    if (typeof(Delegate).IsAssignableFrom(typeToReflect))
                    {
                        MyInternalCopy = (originalObject) =>
                        {
                            var OrginalDelegates = ((Delegate)originalObject).GetInvocationList();
                            var Results = new Delegate[OrginalDelegates.Length];
                            for (int i = 0; i < OrginalDelegates.Length; i++)
                            {
                                Results[i] = (Delegate)OrginalDelegates[i].Clone();
                            }
                            var cloneObject = Delegate.Combine(Results);
                            for (int i = 0; i < OrginalDelegates.Length; i++)
                            {
                                if (OrginalDelegates[i].Target != null)
                                {
                                    var Pos = InternalCopy(OrginalDelegates[i].Target.GetType());
                                    InternalCopys[Pos](OrginalDelegates[i].Target);
                                }
                            }
                        };
                    }
                    else
                    {
                        var fiels = new DynamicAssembly.TypeFields(typeToReflect).Fields;
                        var FieldInfo = new
                            (Action<(object cloneObject, object FieldValue)> Copy,
                             DynamicAssembly.FieldControler Field)[fiels.Length];
                        for (int i = 0; i < fiels.Length; i++)
                        {
                            FieldInfo[i] = ((c) =>
                            {
                                var FieldInternalCopy = InternalCopy(c.FieldValue.GetType());
                                InternalCopys[FieldInternalCopy](c.FieldValue);
                            }, fiels[i]);
                        }

                        var FieldLen = fiels.Length;

                        MyInternalCopy = (originalObject) =>
                        {
                            for (int i = 0; i < FieldLen; i++)
                            {
                                var Info = FieldInfo[i];
                                Clone((obj) => Info.Field.SetValue(originalObject, obj),
                                      () => Info.Field.GetValue(originalObject),
                                      (c) => Info.Copy((originalObject, c)));
                            }
                        };
                    }
                }
                InternalCopys[pos] = MyInternalCopy;
                return pos;
            }
        }

        private static Action<object>[] InternalCopys = new Action<object>[0];
        private static int[] TypeCodes = new int[0];
        private static int[] CopyPoss = new int[0];
    }
}