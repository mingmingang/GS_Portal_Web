using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_partnumber")]

    public class MasterPartNumber
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id { get; set; }
        public Int32? country_id { get; set; }
        public Int32? cust_id { get; set; }
        public string part_number { get; set; }
        public string PN_old_jis { get; set; }
        public string PN_new_jis { get; set; }
        public string PN_batt_segmentation { get; set; }
        public string PN_category_batt { get; set; }
        public Int32? PN_qty_L_pallet { get; set; }
        public Int32? PN_qty_S_pallet { get; set; }
        public Decimal? PN_dry_weight { get; set; }
        public Decimal? PN_wet_weight { get; set; }
        public DateTime? PN_createDate { get; set; }
        public string PN_createBy { get; set; }
        public DateTime? PN_modifDate { get; set; }
        public string PN_modifBy { get; set; }
        public int? PN_status { get; set; }
    }

    public class GetLogPriceData
    {
        public Int32? cust_id { get; set; }
        public string part_number { get; set; }
        public string PN_old_jis { get; set; }
        public string PN_new_jis { get; set; }
        public string PN_batt_segmentation { get; set; }
        public string periodic_price { get; set; }
    }
    public class GetOldJISData
    {
        public string PN_old_jis { get; set; }
        public string PN_new_jis { get; set; }
        public string PN_batt_segmentation { get; set; }
    }
    public class GetBattSegmentData
    {
        public string PN_batt_segmentation { get; set; }
    }
}