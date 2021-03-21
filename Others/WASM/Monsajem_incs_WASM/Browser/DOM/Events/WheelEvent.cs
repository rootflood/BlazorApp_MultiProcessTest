using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM.Events
{

    [Export("WheelEvent", typeof(JSObject))]
    public sealed class WheelEvent : MouseEvent
    {
        internal WheelEvent(JSObject handle) : base(handle) { }

        //public WheelEvent(string typeArg, IWheelEventInit eventInitDict) { }
        [Export("DOM_DELTA_LINE")]
        public double DomDeltaLine { get; internal set; }
        [Export("DOM_DELTA_PAGE")]
        public double DomDeltaPage { get; internal set; }
        [Export("DOM_DELTA_PIXEL")]
        public double DomDeltaPixel { get; internal set; }
        [Export("deltaMode")]
        public double DeltaMode { get; internal set; }
        [Export("deltaX")]
        public double DeltaX { get; internal set; }
        [Export("deltaY")]
        public double DeltaY { get; internal set; }
        [Export("deltaZ")]
        public double DeltaZ { get; internal set; }
        [Export("wheelDelta")]
        public double WheelDelta { get; internal set; }
        [Export("wheelDeltaX")]
        public double WheelDeltaX { get; internal set; }
        [Export("wheelDeltaY")]
        public double WheelDeltaY { get; internal set; }
        [Export("getCurrentPoint")]
        public void GetCurrentPoint(Element element)
        {
            InvokeMethod<object>("getCurrentPoint", element);
        }
        //[Export("initWheelEvent")]
        //public void InitWheelEvent(string typeArg, bool canBubbleArg, bool cancelableArg, Window viewArg, double detailArg, double screenXArg, double screenYArg, double clientXArg, double clientYArg, double buttonArg, EventTarget relatedTargetArg, string modifiersListArg, double deltaXArg, double deltaYArg, double deltaZArg, double deltaMode)
        //{
        //    InvokeMethod<object>("initWheelEvent", typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenYArg, clientXArg, clientYArg, buttonArg, relatedTargetArg, modifiersListArg, deltaXArg, deltaYArg, deltaZArg, deltaMode);
        //}
        
    }

}
