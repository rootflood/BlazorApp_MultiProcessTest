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
    public class CopyOrginalObject : Attribute
    { }
    internal class ObjectContainer :
        IComparable<ObjectContainer>
    {
        public object obj;
        public int TypeHashCode;
        public int ObjHashCode;
        public int FromPos;
        public byte[] Data;

        public Action<object> Set;
        public Func<object> Get;

        public int CompareTo(ObjectContainer other)
        {
            if(TypeHashCode!=other.TypeHashCode)
                return TypeHashCode - other.TypeHashCode;
            return ObjHashCode - other.ObjHashCode;
        }
    }
    public static class ObjectExtensions
    {
        [ThreadStatic]
        private static bool OrginalTargetForDelegates;
        [ThreadStatic]
        private static SortedArray<ObjectContainer> visited;
        [ThreadStatic]
        private static Action AtLast;
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive) |
                type.IsEnum;
        }

        public static Object Copy(this Object originalObject, bool OrginalTargetForDelegates = false)
        {
            ObjectExtensions.OrginalTargetForDelegates = OrginalTargetForDelegates;
            var Pos = InternalCopy(originalObject.GetType());
            visited = new SortedArray<ObjectContainer>(5);
            var Result = InternalCopys[Pos]
                (originalObject);
            AtLast?.Invoke();
            AtLast = null;
            return Result;
        }

        public static T Copy<T>(this T original, bool OrginalTargetForDelegates = false)
        {
            if (original == null) return default(T);
            return (T)Copy(
                originalObject: original,
                OrginalTargetForDelegates: OrginalTargetForDelegates);
        }

        private static void Clone(
            object originalObject,
            Action<object> SetValue,
            Func<object,object> Copy)
        {
            if (originalObject == null)
                return;
            var VisitedObj = new ObjectContainer()
            {
                ObjHashCode = originalObject.GetHashCode()
            };
            var VisitedPos = visited.BinarySearch(VisitedObj);
            if (VisitedPos.Index > -1)
            {
                VisitedObj = visited[VisitedPos.Index];
                AtLast += () => SetValue(VisitedObj.obj);
            }
            else
            {
                visited.BinaryInsert(VisitedObj);
                var Obj = Copy(originalObject);
                SetValue(Obj);
                VisitedObj.obj = Obj;
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
                Insert(ref InternalCopys, (Func<object, object>)null);
                Insert(ref CopyPoss, pos, BinaryInsert(ref TypeCodes, typeToReflect.GetHashCode()));

                Func<object,object> MyInternalCopy;

                if (IsPrimitive(typeToReflect))
                    MyInternalCopy = (originalObject) => originalObject;
                else
                {
                    if(typeToReflect.IsArray| typeToReflect == typeof(System.Array))
                    {
                        Func<object, (Type ElementType, int Rank, 
                            Func<object, object> InternalCopy)> GetInfo=null;
                        Func<object, object>
                            MyInternalCopy_ArrDelegate = (originalObject) => null;
                        Func<object, object> MyInternalCopy_Arr;
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

                                var cloneObject = System.Array.CreateInstance(Info.ElementType, lents);

                                cloneObject.ForEach(lents,(indices) =>
                                {
                                    var StandAloneCurrent = new int[Info.Rank];
                                    for (int i = 0; i < Info.Rank; i++)
                                        StandAloneCurrent[i] = indices[i];
                                    Clone(ArrayObject.GetValue(StandAloneCurrent),
                                        (obj) => cloneObject.SetValue(obj, StandAloneCurrent),
                                        Info.InternalCopy);
                                });
                                visited.BinaryInsert(new ObjectContainer()
                                {
                                    obj = cloneObject,
                                    ObjHashCode = originalObject.GetHashCode()
                                });
                                return cloneObject;
                            };
                        }

                        if (typeToReflect == typeof(System.Array))
                        {
                            MyInternalCopy = (originalObject) =>
                            {
                                var Pos = InternalCopy(originalObject.GetType());
                                return InternalCopys[Pos](originalObject);
                            };
                        }
                        else
                        {
                            var ElementType = typeToReflect.GetElementType();
                            var Rank = typeToReflect.GetArrayRank();
                            var ICPos = InternalCopy(typeToReflect.GetElementType());
                            var ArrayInternalCopy = InternalCopys[ICPos];
                            GetInfo = (ar) => (ElementType, Rank, ArrayInternalCopy);
                            if (IsPrimitive(typeToReflect.GetElementType()))
                            {
                                MyInternalCopy = (ar)=>
                                {
                                    var ArrayObject = (System.Array)ar;
                                    var lents = new int[Rank];
                                    for (int i = 0; i < Rank; i++)
                                        lents[i] = ArrayObject.GetUpperBound(i) + 1;
                                    var cloneObject = System.Array.CreateInstance(ElementType, lents);
                                    ArrayObject.CopyTo(cloneObject, 0);
                                    return cloneObject;
                                };
                            }
                            else
                            {
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
                                        if (OrginalTargetForDelegates)
                                        {
                                            Serialization.Deletage_Target.SetValue(Results[i], OrginalDelegates[i].Target);
                                        }
                                        else
                                        {
                                            var Pos = InternalCopy(OrginalDelegates[i].Target.GetType());
                                            var ClonedTarget = InternalCopys[Pos](OrginalDelegates[i].Target);
                                            Serialization.Deletage_Target.SetValue(Results[i], ClonedTarget);
                                        }
                                    }
                                }

                                return cloneObject;
                            };
                        }
                        else
                        {
                            var Fields = new DynamicAssembly.TypeFields(typeToReflect).Fields;                            
                            var FieldIInfo =new
                                (Func<(object cloneObject,object FieldValue),object> Copy,
                                 DynamicAssembly.FieldControler Field)[Fields.Length];
                            for (int i = 0; i < Fields.Length; i++)
                            {
                                var Field = Fields[i];
                                if (Field.Info.GetCustomAttributes(typeof(CopyOrginalObject)).Count() > 0)
                                    FieldIInfo[i] = ((c)=>
                                    {
                                        var Result = c.FieldValue;
                                        Field.SetValue(c.cloneObject,Result);
                                        return Result;
                                    },Field);
                                else if (!IsPrimitive(Field.Info.FieldType))
                                    FieldIInfo[i] = ((c) =>
                                    {
                                        var FieldInternalCopy = InternalCopy(c.FieldValue.GetType());
                                        var clonedFieldValue = InternalCopys[FieldInternalCopy](c.FieldValue);
                                        Field.SetValue(c.cloneObject, clonedFieldValue);
                                        return clonedFieldValue;
                                    },Field);
                                else
                                {
                                    var Pos = InternalCopy(Field.Info.FieldType);
                                    var Copy = InternalCopys[Pos];
                                    FieldIInfo[i] = ((c) =>
                                    {
                                        var clonedFieldValue = Copy(c.FieldValue);
                                        Field.SetValue(c.cloneObject, clonedFieldValue);
                                        return clonedFieldValue;
                                    },Field);
                                }
                            }

                            var FieldLen = Fields.Length;

                            MyInternalCopy = (originalObject) =>
                            {
                                var cloneObject = GetUninitializedObject(originalObject.GetType());
                                visited.BinaryInsert(new ObjectContainer
                                {
                                    ObjHashCode = originalObject.GetHashCode(),
                                    obj = cloneObject
                                });
                                for (int i = 0; i < FieldLen; i++)
                                {
                                    var Info = FieldIInfo[i];
                                    Clone(Info.Field.GetValue(originalObject),
                                        (c) => Info.Field.SetValue(cloneObject, c),
                                        (c)=> Info.Copy((cloneObject, c)));
                                }
                                return cloneObject;
                            };
                        }
                    }
                }
                InternalCopys[pos] = MyInternalCopy;
                return pos;
            }
        }

        private static Func<object, object>[] InternalCopys = new Func<object, object>[0];
        private static int[] TypeCodes = new int[0];
        private static int[] CopyPoss = new int[0];
    }
}