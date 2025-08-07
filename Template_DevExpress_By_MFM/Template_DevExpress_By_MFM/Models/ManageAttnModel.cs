using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_attn_OEM")]
    public class MasterAttn
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string attn_bp_code { get; set; }
        public string attn_yth { get; set; }
        public string attn_customer { get; set; }
        public string attn_address { get; set; }
        public string attn_to { get; set; }
        public string attn_name { get; set; }
        public DateTime? attn_createDate { get; set; }
        public string attn_createBy { get; set; }
        public DateTime? attn_modifDate { get; set; }
        public string attn_modifBy { get; set; }
        public int? attn_status { get; set; }
    }



}