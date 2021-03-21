using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLDataElement", typeof(JSObject))]
    public sealed class HTMLDataElement : HTMLElement, IHTMLDataElement
    {
        internal HTMLDataElement(JSObject handle) : base(handle) { }

        //public HTMLDataElement() { }
        [Export("value")]
        public string NodeValue { get => GetProperty<string>("value"); set => SetProperty<string>("value", value); }
    }

}