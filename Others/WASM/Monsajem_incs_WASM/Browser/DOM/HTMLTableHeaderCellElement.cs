using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLTableHeaderCellElement", typeof(JSObject))]
    public sealed class HTMLTableHeaderCellElement : HTMLTableCellElement
    {
        internal HTMLTableHeaderCellElement(JSObject handle) : base(handle) { }

        //public HTMLTableHeaderCellElement () { }
        //[Export("scope")]
        //public string Scope { get => GetProperty<string>("scope"); set => SetProperty<string>("scope", value); }
    }
}