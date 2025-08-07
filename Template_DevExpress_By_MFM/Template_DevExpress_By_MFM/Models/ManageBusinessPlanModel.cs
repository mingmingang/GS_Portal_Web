using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("manage_business_plan")]
    public class ManageBusinessPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64? id_recnum_bspln { get; set; }

        public string pn_gs { get; set; }
        public string pn_customer { get; set; }
        public Decimal? qty_1 { get; set; }
        public Decimal? qty_2 { get; set; }
        public Decimal? qty_3 { get; set; }
        public Decimal? qty_4 { get; set; }
        public Decimal? qty_5 { get; set; }
        public Decimal? qty_6 { get; set; }
        public Decimal? qty_7 { get; set; }
        public Decimal? qty_8 { get; set; }
        public Decimal? qty_9 { get; set; }
        public Decimal? qty_10 { get; set; }
        public Decimal? qty_11 { get; set; }
        public Decimal? qty_12 { get; set; }
        public Decimal? qty_total { get; set; }
        public int? tahun { get; set; }
        public DateTime? date_insert { get; set; }
        public DateTime? date_update { get; set; }
        //public Int64? qty_1 { get; set; }
        //public Int64? qty_2 { get; set; }
        //public Int64? qty_3 { get; set; }
        //public Int64? qty_4 { get; set; }
        //public Int64? qty_5 { get; set; }
        //public Int64? qty_6 { get; set; }
        //public Int64? qty_7 { get; set; }
        //public Int64? qty_8 { get; set; }
        //public Int64? qty_9 { get; set; }
        //public Int64? qty_10 { get; set; }
        //public Int64? qty_11 { get; set; }
        //public Int64? qty_12 { get; set; }
        //public Int64? qty_total { get; set; }

    }

}