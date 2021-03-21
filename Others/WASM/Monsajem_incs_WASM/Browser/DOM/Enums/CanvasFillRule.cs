using System;
using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{
    // "nonzero" | "evenodd";
    public enum CanvasFillRule
    {
        [Export(EnumValue = ConvertEnum.ToLower)]
        NonZero,
        [Export(EnumValue = ConvertEnum.ToLower)]
        EventOdd,
    }
}
