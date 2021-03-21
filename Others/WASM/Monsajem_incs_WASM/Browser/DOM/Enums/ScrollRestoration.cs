using System;

using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{
    public enum ScrollRestoration
    {
        [Export(EnumValue = ConvertEnum.ToLower)]
        Auto,
        [Export(EnumValue = ConvertEnum.ToLower)]
        Manual
    }
}
