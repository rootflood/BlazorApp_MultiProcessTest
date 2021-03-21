using System;
using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{
    public partial class EventTarget
    {

        public int DispatchDOMEvent(string typeOfEvent, JSObject eventTarget)
        {

            var eventArgs = new DOMEventArgs(this,typeOfEvent, eventTarget);


            lock (eventHandlers)
            {
                if (eventHandlers.TryGetValue(typeOfEvent, out DOMEventHandler eventHandler))
                {
                    eventHandler?.Invoke(this, eventArgs);
                }
            }

            eventArgs.EventObject?.Dispose();
            eventArgs.EventObject = null;
            eventArgs.Source = null;
            eventArgs = null;
            return 0;
        }
    }
}
