using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLParagraphElement", typeof(JSObject))]
    public sealed class HTMLParagraphElement : HTMLElement, IHTMLParagraphElement
    {
        public HTMLParagraphElement(JSObject handle) : base(handle) { }

        //public HTMLParagraphElement() { }
        [Export("align")]
        public string Align { get => GetProperty<string>("align"); set => SetProperty<string>("align", value); }
        [Export("clear")]
        public string Clear { get => GetProperty<string>("clear"); set => SetProperty<string>("clear", value); }
    }

}