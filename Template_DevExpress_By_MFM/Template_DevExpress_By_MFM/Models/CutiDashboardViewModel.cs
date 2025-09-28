using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    public class CutiDashboardViewModel
    {
        public int HakCutiPribadi { get; set; } = 0;
        public int HakCutiBesar { get; set; } = 0;

        public int PemakaianCutiPribadi { get; set; } = 0;
        public int PemakaianCutiBesar { get; set; } = 0;
        public int OnProgressCutiPribadi { get; set; } = 0;
        public int OnProgressCutiBesar { get; set; } = 0;

        public int SisaCutiPribadi { get; set; } = 0;
        public int SisaCutiBesar { get; set; } = 0;
    }
}