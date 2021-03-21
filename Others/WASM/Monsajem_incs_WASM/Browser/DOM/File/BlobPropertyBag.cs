using System.Runtime.InteropServices.JavaScript;
namespace WebAssembly.Browser.DOM
{
    /// <summary>
    /// A BlobPropertyBag object that provides the properties for the new Blob object.
    /// </summary>
    
    [Export("Object", typeof(JSObject))]
    public class BlobPropertyBag
    {
        /// <summary>
        /// A string representing the MIME type of the Blob object.
        /// </summary>
        public string Type;

        /// <summary>
        /// Specifies how strings containing \n are to be written out. This can be "transparent" (endings unchanged) or "native" (endings changed to match host OS filesystem convention). The default value is "transparent". It corresponds to the endings parameter of the deprecated BlobBuilder.append().
        /// </summary>
        public Endings Endings;
    }
}