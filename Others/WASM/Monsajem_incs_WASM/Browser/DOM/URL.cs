using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{


    [ExportClass("URL", typeof(URL))]
    public sealed class URL : DOMObject
    {
        internal URL(JSObject handle) : base(handle) { }

        //public URL (string url, object base) { }
        [Export("hash")]
        public string Hash { get => GetProperty<string>("hash"); set => SetProperty<string>("hash", value); }
        [Export("host")]
        public string Host { get => GetProperty<string>("host"); set => SetProperty<string>("host", value); }
        [Export("hostname")]
        public string Hostname { get => GetProperty<string>("hostname"); set => SetProperty<string>("hostname", value); }
        [Export("href")]
        public string Href { get => GetProperty<string>("href"); set => SetProperty<string>("href", value); }
        [Export("origin")]
        public string Origin => GetProperty<string>("origin");
        [Export("password")]
        public string Password { get => GetProperty<string>("password"); set => SetProperty<string>("password", value); }
        [Export("pathname")]
        public string Pathname { get => GetProperty<string>("pathname"); set => SetProperty<string>("pathname", value); }
        [Export("port")]
        public string Port { get => GetProperty<string>("port"); set => SetProperty<string>("port", value); }
        [Export("protocol")]
        public string Protocol { get => GetProperty<string>("protocol"); set => SetProperty<string>("protocol", value); }
        [Export("search")]
        public string Search { get => GetProperty<string>("search"); set => SetProperty<string>("search", value); }
        [Export("username")]
        public string Username { get => GetProperty<string>("username"); set => SetProperty<string>("username", value); }
        [Export("searchParams")]
        public URLSearchParams SearchParams => GetProperty<URLSearchParams>("searchParams");
        [Export("createObjectURL")]
        public static string CreateObjectUrl(Blob obj)
        {
            return (string) StaticObject<URL>().Invoke("createObjectURL", obj.ManagedJSObject);
        }
        public static string CreateDataUrl(byte[] Data)
        {
            return "data:;base64,"+Convert.ToBase64String(Data);
        }
        [Export("revokeObjectURL")]
        public static void RevokeObjectUrl(string url)
        {
            StaticObject<URL>().Invoke("revokeObjectURL", url);
        }
        [Export("toString")]
        public override string ToString()
        {
            return InvokeMethod<string>("toString");
        }
    }

}