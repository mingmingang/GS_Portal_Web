using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_kurs")]
    public class MasterKurs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32? kurs_id { get; set; }
        public string kurs_year { get; set; }
        public string kurs_month { get; set; }
        public string kurs_date { get; set; }
        public decimal? kurs_jual { get; set; }
        public decimal? kurs_beli { get; set; }
        public decimal? kurs_average { get; set; }
        public DateTime? kurs_createDate { get; set; }
        public string kurs_createBy { get; set; }
        public DateTime? kurs_modifDate { get; set; }
        public string kurs_modifBy { get; set; }
        public int? kurs_status { get; set; }
    }


    public class MappingKurs
    {
        //public Int32? lme_id { get; set; }
        public string tahun { get; set; }
        public string bulan { get; set; }
        public Int32? tanggal { get; set; }
        public decimal? kurs_jual { get; set; }
        public decimal? kurs_beli { get; set; }
        public decimal? kurs_average { get; set; }
        public decimal? JAN_JUAL { get; set; }
        public decimal? JAN_BELI { get; set; }
        public decimal? JAN_AVG { get; set; }
        public decimal? FEB_JUAL { get; set; }
        public decimal? FEB_BELI { get; set; }
        public decimal? FEB_AVG { get; set; }
        public decimal? MAR_JUAL { get; set; }
        public decimal? MAR_BELI { get; set; }
        public decimal? MAR_AVG { get; set; }
        public decimal? APR_JUAL { get; set; }
        public decimal? APR_BELI { get; set; }
        public decimal? APR_AVG { get; set; }
        public decimal? MAY_JUAL { get; set; }
        public decimal? MAY_BELI { get; set; }
        public decimal? MAY_AVG { get; set; }
        public decimal? JUN_JUAL { get; set; }
        public decimal? JUN_BELI { get; set; }
        public decimal? JUN_AVG { get; set; }
        public decimal? JUL_JUAL { get; set; }
        public decimal? JUL_BELI { get; set; }
        public decimal? JUL_AVG { get; set; }
        public decimal? AUG_JUAL { get; set; }
        public decimal? AUG_BELI { get; set; }
        public decimal? AUG_AVG { get; set; }
        public decimal? SEP_JUAL { get; set; }
        public decimal? SEP_BELI { get; set; }
        public decimal? SEP_AVG { get; set; }
        public decimal? OCT_JUAL { get; set; }
        public decimal? OCT_BELI { get; set; }
        public decimal? OCT_AVG { get; set; }
        public decimal? NOV_JUAL { get; set; }
        public decimal? NOV_BELI { get; set; }
        public decimal? NOV_AVG { get; set; }
        public decimal? DEC_JUAL { get; set; }
        public decimal? DEC_BELI { get; set; }
        public decimal? DEC_AVG { get; set; }
        public DateTime? kurs_createDate { get; set; }
        public string kurs_createBy { get; set; }
        public DateTime? kurs_modifDate { get; set; }
        public string kurs_modifBy { get; set; }
        public int? kurs_status { get; set; }
    }
}