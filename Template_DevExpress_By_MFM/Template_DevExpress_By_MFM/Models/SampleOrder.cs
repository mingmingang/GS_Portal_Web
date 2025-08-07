using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_DevExpress_By_MFM.Models {
    public class OilProduction
    {
        public string Year { get; set; }
        public string Country { get; set; }
        public double Oil { get; set; }
    }

}
    

        //Explicitly setting the name to be used while serializing to JSON.
        //[DataMember(Name = "label")]
        //public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        //[DataMember(Name = "y")]
        //public Nullable<double> Y = null;