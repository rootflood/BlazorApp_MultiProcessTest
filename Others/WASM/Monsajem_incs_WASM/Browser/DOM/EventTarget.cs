using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using WebAssembly.Browser.DOM.Events;
using Monsajem_Incs.Serialization;
using System.Linq;


namespace WebAssembly.Browser.DOM
{


    [Export("EventTarget", typeof(JSObject))]
    public partial class EventTarget : DOMObject, IEventTarget
    {

        static int nextEventId = 0;
        static int NextEventId => nextEventId++;

        internal EventTarget(JSObject handle) : base(handle) { }

        protected EventTarget(string globalName) : base(globalName)
        {

        }

        internal Dictionary<string, DOMEventHandler> eventHandlers = new Dictionary<string, DOMEventHandler>();


        [Export("addEventListener")]
        public void AddEventListener(string type, DOMEventHandler listener, object options)
        {
            foreach (var unmanaged in
                            listener.Target?.Wrap().Wraps.
                            Select((c) => c.Get()).
                            Where((c) => typeof(DOMObject).IsAssignableTo(c.GetType())).
                            Select((c) => (DOMObject)c))
                unmanaged.ReadyForManageObject();

            bool addNativeEventListener = false;
            lock (eventHandlers)
            {
                if (!eventHandlers.ContainsKey(type))
                {
                    eventHandlers.Add(type, null);
                    addNativeEventListener = true;
                }
                eventHandlers[type] += listener;
            }

            if (addNativeEventListener)
            {

                var UID = NextEventId;
                AddJSEventListener(type, dispather(type), UID);

            }


        }

        public delegate int DOMEventDelegate(JSObject eventTarget);


        DOMEventDelegate dispather(string type)
        {
            return (eventTarget) => DispatchDOMEvent(type, eventTarget);
        }

        [Export("dispatchEvent")]
        public bool DispatchEvent(Event evt)
        {
            return InvokeMethod<bool>("dispatchEvent", evt);
        }

        [Export("removeEventListener")]
        public void RemoveEventListener(string type, DOMEventHandler listener, object options)
        {

        }

    }

}