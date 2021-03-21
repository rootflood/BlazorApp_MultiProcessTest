using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using Monsajem_Incs.Array.Hyper;
using Monsajem_Incs.Serialization;
using static System.Runtime.Serialization.FormatterServices;
using Monsajem_Incs.Net.Base.Service;
using System.Threading;

namespace Monsajem_Incs.Database.Base
{
    public class Caption:Attribute
    {
        public string Name_Single { get; set; }
        private string _Name_Multy { get; set; }
        public string Name_Multy
        {
            get => _Name_Multy;
            set
            {
                if (Name_Multy_Of == null)
                    Name_Multy_Of = value;
                _Name_Multy = value;
            }
        }
        public string Name_Multy_Of { get; set; }
        public string Name_Single_Unknown { get; set; }
        public string Name_Multy_Unknown { get; set; }
    }
}