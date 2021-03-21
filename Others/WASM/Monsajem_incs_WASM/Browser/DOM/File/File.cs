using System.IO;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{   
    [Export("File", typeof(JSObject))]
    public class File : Blob
    {
        internal File(JSObject jSObject):base(jSObject){}
        [Export("name")]
        public string Name { get => InvokeMethod<string>("name"); }
    }
}