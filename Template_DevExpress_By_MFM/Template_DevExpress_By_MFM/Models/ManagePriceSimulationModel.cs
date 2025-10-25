using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_price_simulation")]
    public class ManagePriceSimulation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 recnum_id { get; set; }
        public Int32? cust_id { get; set; }

        [MaxLength(50)]
        public string price_sim_id { get; set; }

        [MaxLength(50)]
        public string price_sim_PN { get; set; }
        [MaxLength(50)]
        public string price_sim_old_JIS { get; set; }
        [MaxLength(50)]
        public string price_sim_new_JIS { get; set; }
        public string price_sim_batt_segmentation { get; set; }
        public Int32? price_sim_request_qty { get; set; }
        public Int32? price_sim_qty_pallet_L { get; set; }
        public Int32? price_sim_qty_pallet_S { get; set; }
        public Int32? price_sim_adjust_qty { get; set; }

        public Int32? price_sim_total_pallet_L { get; set; }
        public Int32? price_sim_total_pallet_S { get; set; }

        public Decimal? price_sim_unit_price { get; set; }
        public Decimal? price_sim_amount { get; set; }
        public Decimal? price_sim_batt_weight { get; set; }
        public Decimal? price_sim_total_batt_weight { get; set; }
        public Int32? price_sim_total_container { get; set; }
        public int? price_sim_status { get; set; }
        public DateTime? price_sim_createDate { get; set; }
        public DateTime? price_sim_modifDate { get; set; }
        public string price_sim_createBy { get; set; }
        public string price_sim_modifBy { get; set; }
        public string price_sim_info { get; set; }
    }

    [Table("t_price_simulation_temp")]
    public class ManagePriceSimulation_temp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 recnum_id { get; set; }

        public Int32? cust_id { get; set; }
        public string price_sim_id { get; set; }

        [MaxLength(50)]
        public string price_sim_PN { get; set; }
        [MaxLength(50)]
        public string price_sim_old_JIS { get; set; }
        [MaxLength(50)]
        public string price_sim_new_JIS { get; set; }
        public string price_sim_batt_segmentation { get; set; }
        public Int32? price_sim_request_qty { get; set; }
        public Int32? price_sim_qty_pallet_L { get; set; }
        public Int32? price_sim_qty_pallet_S { get; set; }
        public Int32? price_sim_adjust_qty { get; set; }

        public Int32? price_sim_total_pallet_L { get; set; }
        public Int32? price_sim_total_pallet_S { get; set; }

        public Decimal? price_sim_unit_price { get; set; }
        public Decimal? price_sim_amount { get; set; }
        public Decimal? price_sim_batt_weight { get; set; }
        public Decimal? price_sim_total_batt_weight { get; set; }
        public Int32? price_sim_total_container { get; set; }
        public int? price_sim_status { get; set; }
        public DateTime? price_sim_createDate { get; set; }
        public DateTime? price_sim_modifDate { get; set; }
        public string price_sim_createBy { get; set; }
        public string price_sim_modifBy { get; set; }
    }

    public class GetLastPriceID
    {
        public string price_sim_id { get; set; }
    }
    public class GetAVGLME
    {
        public decimal? avg_lme { get; set; }
    }
    public class GetPrevPrice
    {
        public decimal? prev_price { get; set; }
    }

    //public class ListGroupManageOrder
    //{
    //    public string id_order { get; set; }
    //    public int status_order { get; set; }
    //}

    public class ListHeaderManagePriceSimulation
    {
        public string price_sim_id { get; set; }
        public string po_number { get; set; }
        public DateTime? price_sim_createDate { get; set; }
        public DateTime? price_sim_modifDate { get; set; }
        public string month_order { get; set; }
        public string year_order { get; set; }
        public int price_sim_status { get; set; }
        public int stat { get; set; }
    }

    //public class StatusOrder {
    //    public int ID { get; set; }
    //    public string Name { get; set; }
    //}


    //[Table("manage_doc_order")]
    //public class ManageDocumentOrder
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long id_recnum { get; set; }
    //    public string id_order { get; set; }
    //    public string doc { get; set; }
    //}

}