using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("DOMSettableTokenList", typeof(JSObject))]
    public sealed class DOMSettableTokenList : DOMTokenList, IDOMSettableTokenList
    {
        internal DOMSettableTokenList(JSObject handle) : base(handle) { }

        //public DOMSettableTokenList() { }
        [Export("value")]
        public string NodeValue { get => GetProperty<string>("value"); set => SetProperty<string>("value", value); }
    }
}