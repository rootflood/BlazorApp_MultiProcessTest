using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("CDATASection", typeof(JSObject))]
    public sealed class CDATASection : Text
    {
        internal CDATASection(JSObject handle) : base(handle) { }

        //public CDATASection() { }
    }
}