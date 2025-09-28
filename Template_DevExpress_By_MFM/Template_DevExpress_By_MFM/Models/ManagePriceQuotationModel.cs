using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_price_quotation")]
    public class ManagePriceQuotation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id { get; set; }

        public string customer_bpid { get; set; }
        public string customer_name { get; set; }

        public string quotation_period { get; set; }
        public string battery_type { get; set; }
        public string part_number { get; set; }

        public string quotation_createBy { get; set; }

        public string quotation_modifBy { get; set; }

        public Decimal? LME_lead { get; set; }
        public int quotation_status { get; set; }
        public DateTime? quotation_createDate { get; set; }
        public DateTime? quotation_modifDate { get; set; }
        public Int32 premium1 { get; set; }
        public Int32 premium2 { get; set; }
        public Int32 premium3 { get; set; }
        public Decimal? plastic_pp { get; set; }
        public Int32 ex_rate { get; set; }
        public int? import_duty { get; set; }
        public int? handling_fee { get; set; }
        public int? import_duty2 { get; set; }
        public int? handling_fee2 { get; set; }
        public int? import_duty3 { get; set; }
        public int? handling_fee3 { get; set; }
        public int? import_duty_plastic { get; set; }
        public int? handling_fee_plastic { get; set; }
        public Decimal? material_weight1 { get; set; }
        public Decimal? lpp_fee1 { get; set; }
        public Decimal? lead_premium1 { get; set; }
        public Decimal? material_weight2 { get; set; }
        public Decimal? lpp_fee2 { get; set; }
        public Decimal? lead_premium2 { get; set; }
        public Decimal? material_weight3 { get; set; }
        public Decimal? lpp_fee3 { get; set; }
        public Decimal? lead_premium3 { get; set; }
        public Decimal? plastic_weight { get; set; }
        public Decimal? pp_price { get; set; }
        public Decimal? plastic { get; set; }
        public Decimal? separator { get; set; }
        public Decimal? others_purchase { get; set; }
        public Decimal? sub_total_mat_cost { get; set; }
        public Decimal? process_plate { get; set; }
        public Decimal? process_injection { get; set; }
        public Decimal? process_assembling { get; set; }
        public Decimal? process_charging { get; set; }
        public Decimal? sub_total_process_cost { get; set; }
        public Decimal? total { get; set; }
        public Decimal? general_charge { get; set; }
        public Decimal? others { get; set; }
        public Decimal? support { get; set; }
        public Decimal? grand_total { get; set; }
    }
}