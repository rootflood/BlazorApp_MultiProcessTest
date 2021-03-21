using System;

using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{
    public class ElementAttributes: DOMObject
    {
        public ElementAttributes(JSObject handle) : base(handle) { }

        public string this[string qualifiedName]
        {
            get
            {
                if (string.IsNullOrEmpty(qualifiedName))
                    throw new ArgumentNullException(nameof(qualifiedName));

                return InvokeMethod<string>("getAttribute", qualifiedName);
            }
            set
            {
                if (string.IsNullOrEmpty(qualifiedName))
                    throw new ArgumentNullException(nameof(qualifiedName));

                InvokeMethod<string>("setAttribute", qualifiedName, value);
            }
        }
    }

    public partial class Element
    {
        // Special Attribute and Style methods
        #region Attribute and Style methods

        public ElementAttributes Attribute { get => new ElementAttributes(this.ManagedJSObject); }

        [Export("getAttribute")]
        public string GetAttribute(string qualifiedName)=>
            Attribute[qualifiedName];

        [Export("setAttribute")]
        public void SetAttribute(string qualifiedName, string value) =>
            Attribute[qualifiedName] = value;

        [Export("removeAttribute")]
        public void RemoveAttribute(string qualifiedName)
        {

            if (string.IsNullOrEmpty(qualifiedName))
                throw new ArgumentNullException(nameof(qualifiedName));

            InvokeMethod<string>("removeAttribute", qualifiedName);
        }

        public void SetStyleAttribute(string qualifiedName, string value)
        {

            if (string.IsNullOrEmpty(qualifiedName))
                throw new ArgumentNullException(nameof(qualifiedName));

            SetJSStyleAttribute(qualifiedName, value);
        }


        public string GetStyleAttribute(string qualifiedName)
        {
            if (string.IsNullOrEmpty(qualifiedName))
                throw new ArgumentNullException(nameof(qualifiedName));

            return GetJSStyleAttribute(qualifiedName).ToString();
        }


        public void RemoveStyleAttribute(string qualifiedName)
        {
            if (string.IsNullOrEmpty(qualifiedName))
                throw new ArgumentNullException(nameof(qualifiedName));

            SetJSStyleAttribute(qualifiedName, null);

        }


        #endregion
    }
}
