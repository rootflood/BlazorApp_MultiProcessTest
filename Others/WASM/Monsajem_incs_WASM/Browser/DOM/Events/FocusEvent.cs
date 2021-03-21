﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM.Events
{

    [Export("FocusEvent", typeof(JSObject))]
    public sealed class FocusEvent : UIEvent
    {
        internal FocusEvent(JSObject handle) : base(handle) { }

        //public FocusEvent (string typeArg, FocusEventInit eventInitDict) { }
        [Export("relatedTarget")]
        public EventTarget RelatedTarget => GetProperty<EventTarget>("relatedTarget");
        //[Export("initFocusEvent")]
        //public void InitFocusEvent(string typeArg, bool canBubbleArg, bool cancelableArg, Window viewArg, double detailArg, EventTarget relatedTargetArg)
        //{
        //	InvokeMethod<object>("initFocusEvent", typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, relatedTargetArg);
        //}

    }


}