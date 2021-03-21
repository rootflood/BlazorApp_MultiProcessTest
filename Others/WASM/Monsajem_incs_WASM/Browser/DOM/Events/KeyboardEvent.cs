using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM.Events
{


    [Export("KeyboardEvent", typeof(JSObject))]
    public sealed class KeyboardEvent : UIEvent
    {
        internal KeyboardEvent(JSObject handle) : base(handle) { }

        //public KeyboardEvent(string typeArg, KeyboardEventInit eventInitDict) { }
        [Export("DOM_KEY_LOCATION_JOYSTICK")]
        public double DomKeyLocationJoystick { get; internal set; }
        [Export("DOM_KEY_LOCATION_LEFT")]
        public double DomKeyLocationLeft { get; internal set; }
        [Export("DOM_KEY_LOCATION_MOBILE")]
        public double DomKeyLocationMobile { get; internal set; }
        [Export("DOM_KEY_LOCATION_NUMPAD")]
        public double DomKeyLocationNumpad { get; internal set; }
        [Export("DOM_KEY_LOCATION_RIGHT")]
        public double DomKeyLocationRight { get; internal set; }
        [Export("DOM_KEY_LOCATION_STANDARD")]
        public double DomKeyLocationStandard { get; internal set; }
        [Export("altKey")]
        public bool AltKey { get; internal set; }
        [Export("ctrlKey")]
        public bool CtrlKey { get; internal set; }
        [Export("keyCode")]
        public double KeyCode { get; internal set; }
        [Export("locale")]
        public string Locale { get; internal set; }
        [Export("location")]
        public double Location { get; internal set; }
        [Export("metaKey")]
        public bool MetaKey { get; internal set; }
        [Export("repeat")]
        public bool Repeat { get; internal set; }
        [Export("shiftKey")]
        public bool ShiftKey { get; internal set; }
        [Export("which")]
        public double Which { get; internal set; }
        [Export("code")]
        public string Code { get; internal set; }
        [Export("getModifierState")]
        public bool GetModifierState(string keyArg)
        {
            return InvokeMethod<bool>("getModifierState", keyArg);
        }
        //[Export("initKeyboardEvent")]
        //public void InitKeyboardEvent(string typeArg, bool canBubbleArg, bool cancelableArg, Window viewArg, string keyArg, double locationArg, string modifiersListArg, bool repeat, string locale)
        //{
        //    InvokeMethod<object>("initKeyboardEvent", typeArg, canBubbleArg, cancelableArg, viewArg, keyArg, locationArg, modifiersListArg, repeat, locale);
        //}
    }
}