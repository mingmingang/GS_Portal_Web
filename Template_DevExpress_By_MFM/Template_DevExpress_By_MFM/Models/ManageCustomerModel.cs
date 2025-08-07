using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_cust")]
    public class MasterCustomer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 customer_id { get; set; }
        public string customer_name { get; set; }
        public string customer_username { get; set; }
        public string customer_password { get; set; }
        public string customer_batt_category { get; set; }
        public string customer_periodic_price { get; set; }
        public int? country_id { get; set; }
        public DateTime? customer_createDate { get; set; }
        public string customer_createBy { get; set; }
        public DateTime? customer_modifDate { get; set; }
        public string customer_modifBy { get; set; }
        public int? customer_status { get; set; }
    }



}