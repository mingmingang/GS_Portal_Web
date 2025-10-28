using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("manage_yearly_plan")]
    public class ManageYearlyPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64? id_recnum_yrpln { get; set; }

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
        public DateTime? date_insert{ get; set; }
        public DateTime? date_update { get; set; }

    }

}