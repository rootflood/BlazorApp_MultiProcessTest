﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

namespace WebAssembly.Browser.DOM.Events
{

    [Export("ClipboardEvent", typeof(JSObject))]
    public sealed class ClipboardEvent : Event
    {
        internal ClipboardEvent(JSObject handle) : base(handle) { }

        //public ClipboardEvent (string type, ClipboardEventInit eventInitDict) { }
        DataTransfer clipboardData;

        [Export("clipboardData")]
        public DataTransfer ClipboardData
        {
            get
            {
                if (clipboardData == null)
                    clipboardData = GetProperty<DataTransfer>("clipboardData");
                return clipboardData;
            }
            set => SetProperty<DataTransfer>("clipboardData", value);
        }


        protected override void Dispose(bool disposing)
        {
            // the event object handle is already unregistered within the event handling function
            // no need to do this again.
            if (disposing)
            {
                if (clipboardData != null)
                {
                    clipboardData.Dispose();
                }
            }

        }
    }
}