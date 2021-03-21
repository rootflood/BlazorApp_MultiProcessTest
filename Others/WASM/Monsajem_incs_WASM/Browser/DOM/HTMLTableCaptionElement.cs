using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLTableCaptionElement", typeof(JSObject))]
    public sealed class HTMLTableCaptionElement : HTMLElement, IHTMLTableCaptionElement
    {
        internal HTMLTableCaptionElement(JSObject handle) : base(handle) { }

        //public HTMLTableCaptionElement () { }
        [Export("align")]
        public string Align { get => GetProperty<string>("align"); set => SetProperty<string>("align", value); }
        [Export("vAlign")]
        public string VAlign { get => GetProperty<string>("vAlign"); set => SetProperty<string>("vAlign", value); }
    }
}