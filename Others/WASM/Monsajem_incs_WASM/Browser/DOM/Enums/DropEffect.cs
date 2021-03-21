using System;

using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{
    public enum DropEffect
    {
        [Export(EnumValue = ConvertEnum.ToLower)]
        None,
        [Export(EnumValue = ConvertEnum.ToLower)]
        Copy,
        [Export(EnumValue = ConvertEnum.ToLower)]
        Link,
        [Export(EnumValue = ConvertEnum.ToLower)]
        Move

    }
}
