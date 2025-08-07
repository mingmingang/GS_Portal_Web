using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{

    public class DashboardBarForecastModel
    {
        public string year_forecast { get; set; }
        public string month_forecast { get; set; }
        public DateTime? date_forecast { get; set; }
        public Decimal yearly_plan { get; set; }
        public Decimal n4 { get; set; }
        public Decimal n3 { get; set; }
        public Decimal n2 { get; set; }
        public Decimal order_qty { get; set; }
        public Decimal n4_FIX { get; set; }
        public Decimal n3_FIX { get; set; }
        public Decimal n2_FIX { get; set; }

    }

}