using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_lme")]
    public class MasterLME
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32? lme_id { get; set; }
        public string lme_year { get; set; }
        public string lme_month { get; set; }
        public string lme_date { get; set; }
        public decimal? lme_value { get; set; }
        public DateTime? lme_createDate { get; set; }
        public string lme_createBy { get; set; }
        public DateTime? lme_modifDate { get; set; }
        public string lme_modifBy { get; set; }
        public int? lme_status { get; set; }
    }


    public class MappingLME
    {
        //public Int32? lme_id { get; set; }
        public string tahun { get; set; }
        public string bulan { get; set; }
        public Int32? tanggal { get; set; }
        public decimal? JAN { get; set; }
        public decimal? FEB { get; set; }
        public decimal? MAR { get; set; }
        public decimal? APR { get; set; }
        public decimal? MAY { get; set; }
        public decimal? JUN { get; set; }
        public decimal? JUL { get; set; }
        public decimal? AUG { get; set; }
        public decimal? SEP { get; set; }
        public decimal? OCT { get; set; }
        public decimal? NOV { get; set; }
        public decimal? DEC { get; set; }
        public int? lme_status { get; set; }
    }
}