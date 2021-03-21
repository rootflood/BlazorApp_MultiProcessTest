using System;
using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{
    public enum Direction
    {
        [Export(EnumValue = ConvertEnum.ToLower)]
        Inherit,
        [Export("rtl")]
        RightToLeft,
        [Export("ltr")]
        LeftToRight,
    }
}
