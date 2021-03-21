using System;
using System.Collections.Generic;
using WebAssembly.Browser.DOM.Events;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{
    public class DOMEventArgs : EventArgs
    {

        public int ClientX { get => (int)Me.GetObjectProperty("clientX"); }
        public int ClientY { get => (int)Me.GetObjectProperty("clientY"); }
        public int OffsetX { get => (int)Me.GetObjectProperty("offsetX"); }
        public int OffsetY { get => (int)Me.GetObjectProperty("offsety"); }
        public int ScreenX { get => (int)Me.GetObjectProperty("screenX"); }
        public int ScreenY { get => (int)Me.GetObjectProperty("Screeny"); }
        public bool AltKey { get => throw new NotImplementedException("Please Declare this!"); }
        public bool CtrlKey { get => throw new NotImplementedException("Please Declare this!"); }
        public bool ShiftKey { get => throw new NotImplementedException("Please Declare this!"); }
        public int KeyCode { get => throw new NotImplementedException("Please Declare this!"); }
        public string EventType { get; internal set; }
        public DOMObject Source { get; internal set; }
        public Event EventObject { get; internal set; }
        private JSObject Me;

        public DOMEventArgs(DOMObject source, string typeOfEvent, JSObject eventObject)
        {

            Me = eventObject;
            Source = source;
            EventType = typeOfEvent;

            switch (typeOfEvent)
            {
                case "MouseEvent":
                    EventObject = new MouseEvent(eventObject);
                    break;
                case "DragEvent":
                    EventObject = new DragEvent(eventObject);
                    break;
                case "FocusEvent":
                    EventObject = new FocusEvent(eventObject);
                    break;
                case "WheelEvent":
                    EventObject = new WheelEvent(eventObject);
                    break;
                case "KeyboardEvent":
                    EventObject = new KeyboardEvent(eventObject);
                    break;
                case "ClipboardEvent":
                    EventObject = new ClipboardEvent(eventObject);
                    break;
                default:
                    EventObject = new Event(eventObject);
                    break;
            }
        }

        public void PreventDefault()
        {
            if (EventObject != null)
                EventObject.PreventDefault();
        }

        public void StopPropagation()
        {
            if (EventObject != null)
                EventObject.StopPropagation();
        }

    }
}
