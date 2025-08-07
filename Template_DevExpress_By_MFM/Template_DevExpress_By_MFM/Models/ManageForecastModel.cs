using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("manage_forecast")]
    public class ManageForecast
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64? id_recnum_forecast { get; set; }

        [MaxLength(100)]
        public string id_forecast { get; set; }

        [MaxLength(20)]
        public string month_forecast { get; set; }

        [MaxLength(10)]
        public string year_forecast { get; set; }

        [MaxLength(100)]
        public string pn_customer { get; set; }

        [MaxLength(100)]
        public string pn_gs { get; set; }
        public Int64? lot_size { get; set; }
        public Decimal? yearly_plan { get; set; }
        public Decimal? n4 { get; set; }
        public Decimal? n3 { get; set; }
        public Decimal? n2 { get; set; }

        [MaxLength(100)]
        public string type_battery { get; set; }

        [MaxLength(100)]
        public string type_material { get; set; }

        [MaxLength(100)]
        public string brand { get; set; }

        [MaxLength(100)]
        public string group_forecast { get; set; }

        [MaxLength(100)]
        public string spec { get; set; }

        public DateTime? insert_time { get; set; }
        public DateTime? update_time { get; set; }

        [MaxLength(50)]
        public string user_input { get; set; }

        [MaxLength(50)]
        public string status_forecast { get; set; }

        public DateTime? date_forecast { get; set; }

    }


    public class ListGroupManageForecast
    {
        public Int64 id_recnum_forecast { get; set; }
        public string id_forecast { get; set; }
    }


    public class TempManageForecast
    {
        public Int64? id_recnum_forecast { get; set; }

        public string id_forecast { get; set; }
        public string id_forecast2 { get; set; }

        public string month_forecast { get; set; }

        public string year_forecast { get; set; }

        public string pn_customer { get; set; }

        public string pn_gs { get; set; }
        public Int64 lot_size { get; set; }
        public Decimal? yearly_plan { get; set; }
        public Decimal? n4 { get; set; }
        public Decimal? n3 { get; set; }
        public Decimal? n2 { get; set; }

        public string type_battery { get; set; }

        public string type_material { get; set; }

        public string brand { get; set; }

        public string group_forecast { get; set; }

        public string spec { get; set; }

        public DateTime? insert_time { get; set; }
        public DateTime? update_time { get; set; }

        public string user_input { get; set; }

        public string status_forecast { get; set; }
        public DateTime? date_forecast { get; set; }

    }

    public class GetLastForecastID
    {
        public string id_forecast { get; set; }
    }

    public class ListYears
    {
        public string years { get; set; }
    }
    public class ListMonths
    {
        public string id_recnum_month { get; set; }
        public string months { get; set; }
    }

}