using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("CanvasPattern", typeof(JSObject))]
    public sealed class CanvasPattern : DOMObject, ICanvasPattern
    {
        internal CanvasPattern(JSObject handle) : base(handle) { }

        //public CanvasPattern() { }
        //[Export("setTransform")]
        //public void SetTransform(SVGMatrix matrix)
        //{
        //    InvokeMethod<object>("setTransform", matrix);
        //}
    }
}