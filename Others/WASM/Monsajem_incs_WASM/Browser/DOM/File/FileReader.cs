using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{
    [Export("FileReader", typeof(JSObject))]
    public class FileReader : EventTarget
    {
        internal FileReader(JSObject handle) : base(handle) { }

        public FileReader() : this(new JSObject(Runtime.New("FileReader"),true)){ }

        [Export("readAsDataURL")]
        public void ReadAsDataURL(Blob selectors)
        {
            InvokeMethod<object>("readAsDataURL", selectors);
        }
        [Export("readAsArrayBuffer")]
        public void ReadAsArrayBuffer(Blob selectors)
        {
            InvokeMethod<object>("readAsArrayBuffer", selectors);
        }
        [Export("readAsText")]
        public void ReadAsText(Blob selectors)
        {
            InvokeMethod<object>("readAsText", selectors);
        }
        [Export("abort")]
        public void Abort()
        {
            InvokeMethod<object>("abort");
        }

        public ArrayBuffer Result { get => GetProperty<ArrayBuffer>("result"); }

        public event DOMEventHandler OnLoadStart
        {
            add => AddEventListener("onloadstart", value, false);
            remove => RemoveEventListener("onloadstart", value, false);
        }
        public event DOMEventHandler OnProgress
        {
            add => AddEventListener("onprogress", value, false);
            remove => RemoveEventListener("onprogress", value, false);
        }
        public event DOMEventHandler OnAbort
        {
            add => AddEventListener("onabort", value, false);
            remove => RemoveEventListener("onabort", value, false);
        }
        public event DOMEventHandler OnError
        {
            add => AddEventListener("onerror", value, false);
            remove => RemoveEventListener("onerror", value, false);
        }
        public event DOMEventHandler OnLoad
        {
            add => AddEventListener("onload", value, false);
            remove => RemoveEventListener("onload", value, false);
        }
        public event DOMEventHandler OnLoadEnd
        {
            add => AddEventListener("onloadend", value, false);
            remove => RemoveEventListener("onloadend", value, false);
        }
    }
}