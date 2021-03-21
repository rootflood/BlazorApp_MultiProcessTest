using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLTimeElement", typeof(JSObject))]
    public sealed class HTMLTimeElement : HTMLElement, IHTMLTimeElement
    {
        internal HTMLTimeElement(JSObject handle) : base(handle) { }

        //public HTMLTimeElement () { }
        [Export("dateTime")]
        public string DateTime { get => GetProperty<string>("dateTime"); set => SetProperty<string>("dateTime", value); }
    }

}