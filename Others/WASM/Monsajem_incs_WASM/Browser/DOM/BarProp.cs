using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("BarProp", typeof(JSObject))]
    public sealed class BarProp : DOMObject
    {
        internal BarProp(JSObject handle) : base(handle) { }

        //public BarProp() { }
        [Export("visible")]
        public bool Visible => GetProperty<bool>("visible");
    }
}