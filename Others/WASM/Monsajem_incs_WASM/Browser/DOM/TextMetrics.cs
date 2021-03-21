using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("TextMetrics", typeof(JSObject))]
    public sealed class TextMetrics : DOMObject, ITextMetrics
    {
        internal TextMetrics(JSObject handle) : base(handle) { }

        //public TextMetrics() { }
        [Export("width")]
        public double Width => GetProperty<double>("width");
    }
}