using System;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM
{

    [Export("HTMLOutputElement", typeof(JSObject))]
    public sealed class HTMLOutputElement : HTMLElement, IHTMLOutputElement
    {
        internal HTMLOutputElement(JSObject handle) : base(handle) { }

        //public HTMLOutputElement () { }
        [Export("defaultNodeValue")]
        public string DefaultNodeValue { get => GetProperty<string>("defaultNodeValue"); set => SetProperty<string>("defaultNodeValue", value); }
        [Export("form")]
        public HTMLFormElement Form => GetProperty<HTMLFormElement>("form");
        [Export("htmlFor")]
        public DOMSettableTokenList HtmlFor => GetProperty<DOMSettableTokenList>("htmlFor");
        [Export("name")]
        public string Name { get => GetProperty<string>("name"); set => SetProperty<string>("name", value); }
        [Export("type")]
        public string Type => GetProperty<string>("type");
        [Export("validationMessage")]
        public string ValidationMessage => GetProperty<string>("validationMessage");
        [Export("validity")]
        public ValidityState Validity => GetProperty<ValidityState>("validity");
        [Export("value")]
        public string NodeValue { get => GetProperty<string>("value"); set => SetProperty<string>("value", value); }
        [Export("willValidate")]
        public bool WillValidate => GetProperty<bool>("willValidate");
        [Export("checkValidity")]
        public bool CheckValidity()
        {
            return InvokeMethod<bool>("checkValidity");
        }
        [Export("reportValidity")]
        public bool ReportValidity()
        {
            return InvokeMethod<bool>("reportValidity");
        }
        [Export("setCustomValidity")]
        public void SetCustomValidity(string error)
        {
            InvokeMethod<object>("setCustomValidity", error);
        }
    }
}