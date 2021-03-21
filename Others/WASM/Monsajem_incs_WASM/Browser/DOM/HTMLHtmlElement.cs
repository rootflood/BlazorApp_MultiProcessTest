using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLHTMLElement", typeof(JSObject))]
    public sealed class HTMLHTMLElement : HTMLElement, IHTMLHTMLElement
    {
        internal HTMLHTMLElement(JSObject handle) : base(handle) { }

        //public HTMLHTMLElement() { }
        [Export("version")]
        public string Version { get => GetProperty<string>("version"); set => SetProperty<string>("version", value); }
    }
}