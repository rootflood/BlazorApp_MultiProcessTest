using System;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using Monsajem_Incs.Array.Hyper;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;

namespace WebAssembly.Browser.DOM
{
    public abstract class DOMObject : IDisposable,IComparable<DOMObject>
    {
        internal static JSObject StaticObject<type>()
        {
            return ExportClassAttribute.GetExportOf<type>().jSObjectStatic;
        }

        internal static SortedArray<DOMObject> objects =
            new SortedArray<DOMObject>(100);

        bool disposed = false;

        public JSObject ManagedJSObject { get; private set; }

        public int JSHandle
        {
            get { return ManagedJSObject.JSHandle;  }
        }

        private Action onRemoved;

        internal void ReadyForManageObject()
        {
            if(objects.BinarySearch(this).Index<0)
                objects.BinaryInsert(this);
            //onRemoved = () => Console.WriteLine(JSHandle);
            //int last = GetProperty<int>("MNH");
            //if(last!=1)
            //    SetProperty("onRemoved", onRemoved);
            //ManagedJSObject.Invoke("onRemoved");
        }
        public DOMObject(JSObject jsObject)
        {
            ManagedJSObject = jsObject;
            ReadyForManageObject();
        }

        public DOMObject(string globalName)
        {
            ManagedJSObject = (JSObject)Runtime.GetGlobalObject(globalName);
            ReadyForManageObject();
        }

        protected object InvokeMethod(Type type, string methodName, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                Type argType = null;
                // All DOMObjects will need to pass the JSObject that they are associated with
                for (int a = 0; a < args.Length; a++)
                {
                    argType = args[a].GetType();
                    if (argType.IsSubclassOf(typeof(DOMObject)) || argType == typeof(DOMObject))
                    {
                        args[a] = ((DOMObject)args[a]).ManagedJSObject;
                    }
                }
            }
            if (ManagedJSObject == null)
                throw new Exception("JSObject Is null");
            var res = ManagedJSObject.Invoke(methodName, args);
            return UnWrapObject(type, res);
        }

        protected T InvokeMethod<T>(string methodName, params object[] args)
        {
            return (T)InvokeMethod(typeof(T), methodName, args);
        }

        protected T GetProperty<T>(string expr)
        {

            var propertyNodeValue = ManagedJSObject.GetObjectProperty(expr);

            if (propertyNodeValue == null)
                return default;
            if (typeof(T) == typeof(object))
                return (T)propertyNodeValue;
            if(typeof(T) == propertyNodeValue.GetType())
                return (T)propertyNodeValue;
            return UnWrapObject<T>(propertyNodeValue);
        }

        protected void SetProperty<T>(string expr, T value, bool createIfNotExists = true, bool hasOwnProperty = false)
        {
            if (value == null)
                ManagedJSObject.SetObjectProperty(expr, value, createIfNotExists, hasOwnProperty);
            else
            {
                var valueType = value.GetType();

                if (valueType.IsSubclassOf(typeof(DOMObject)) || valueType == typeof(DOMObject))
                {
                    ManagedJSObject.SetObjectProperty(expr, ((DOMObject)(object)value).ManagedJSObject, createIfNotExists, hasOwnProperty);
                }
                else
                    ManagedJSObject.SetObjectProperty(expr, value, createIfNotExists, hasOwnProperty);
            }
        }

        object UnWrapObject(Type type, object obj)
        {

            if (type.IsSubclassOf(typeof(JSObject)) || type == typeof(JSObject))
            {


                var jsobjectconstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                null, new Type[] { typeof(Int32) }, null);

                var jsobjectnew = jsobjectconstructor.Invoke(new object[] { (obj == null) ? -1 : obj });
                return jsobjectnew;

            }
            else if (type.IsSubclassOf(typeof(DOMObject)) || type == typeof(DOMObject))
            {


                var jsobjectconstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                null, new Type[] { typeof(JSObject) }, null);

                //var jsobjectnew = jsobjectconstructor.Invoke(new object[] { obj });
                return jsobjectconstructor.Invoke(new object[] { obj }); ;

            }
            else if (type.IsPrimitive || typeof(Decimal) == type)
            {

                // Make sure we handle null and undefined
                // have found this only on FireFox for now
                if (obj == null)
                {
                    return Activator.CreateInstance(type);
                }

                return Convert.ChangeType(obj, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {

                var conv = System.ComponentModel.TypeDescriptor.GetConverter(type);

                if (!conv.CanConvertFrom(obj.GetType()))
                {
                    throw new NotSupportedException();
                }

                if (conv.IsValid(obj))
                {
                    return conv.ConvertFrom(obj);
                }

                throw new InvalidCastException();
            }
            else if (type.IsEnum)
            {
                return obj;
                //return Runtime.EnumFromExportContract(type, obj);
            }
            else if (type == typeof(string))
            {
                return obj;
            }
            else if (type is object)
            {
                // called via invoke
                if (obj == null)
                    return (object)null;
                else
                    throw new NotSupportedException($"Type {type} not supported yet.");

            }
            else
            {
                throw new NotSupportedException($"Type {type} not supported yet.");
            }


        }

        T UnWrapObject<T>(object obj)
        {

            return (T)UnWrapObject(typeof(T), obj);
        }

        private object[] Events = new object[0];
        protected void AddJSEventListener(string eventName, object eventDelegate, int uid)
        {
            ManagedJSObject.Invoke("addEventListener", eventName, eventDelegate, uid);
            Insert(ref Events, eventDelegate);
        }

        protected void SetJSStyleAttribute(string qualifiedName, string value)
        {

            ((JSObject)ManagedJSObject.GetObjectProperty("style")).SetObjectProperty(qualifiedName, value);

        }

        protected object GetJSStyleAttribute(string qualifiedName)
        {
            return ((JSObject)ManagedJSObject.GetObjectProperty("style")).GetObjectProperty(qualifiedName);

        }


        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                    // Free any other managed objects here.
                    //
                }

                ManagedJSObject?.Dispose();
                ManagedJSObject = null;

                disposed = true;
            }
        }

        public int CompareTo(DOMObject other)
        {
            return GetHashCode()-other.GetHashCode();
        }

        // We are hanging onto JavaScript objects and pointers.
        // Make sure the object sticks around long enough or those
        // same objects may get disposed out from under you.
        ~DOMObject()
        {
            Dispose(false);
        }
    }

}
