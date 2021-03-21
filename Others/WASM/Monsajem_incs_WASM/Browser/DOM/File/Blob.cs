
using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{
    
    [Export("Object", typeof(JSObject))]
    public class BlobDataObject
    {
        public static implicit operator BlobDataObject(ArrayBuffer t)
        {
            return null;
        }

        public static implicit operator BlobDataObject(Blob t)
        {
            return null;
        }

        public static implicit operator BlobDataObject(string t)
        {
            return null;
        }

        public static explicit operator ArrayBuffer(BlobDataObject value)
        {
            return default(ArrayBuffer);
        }

        public static explicit operator Blob(BlobDataObject value)
        {
            return default(Blob);
        }

        public static explicit operator string (BlobDataObject value)
        {
            return default(string);
        }
    }
    
    [Export("Blob", typeof(JSObject))]
    public class Blob:DOMObject
    {
        public Blob(byte[] Data) :
            this(((System.Func<JSObject>)(()=>
            {
                var Uint8AR = Uint8Array.From(Data);
                return new JSObject(Runtime.New("Blob", new Array(Uint8AR), new { type = "application/octet-stream" }),true);
            }))())
        { }
        internal Blob(JSObject jSObject) : base(jSObject) { }
    }
}