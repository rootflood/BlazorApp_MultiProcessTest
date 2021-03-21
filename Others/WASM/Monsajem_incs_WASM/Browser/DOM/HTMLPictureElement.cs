using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLPictureElement", typeof(JSObject))]
    public sealed class HTMLPictureElement : HTMLElement
    {
        internal HTMLPictureElement(JSObject handle) : base(handle) { }

        //public HTMLPictureElement() { }
    }
}