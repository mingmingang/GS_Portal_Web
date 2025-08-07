using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_activity_marketing")]
    public class ManageActivityMarketing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 activity_id { get; set; }
        public Int32 id_idl { get; set; }
        public string activity_pic { get; set; }
        public DateTime? activity_date { get; set; }
        public string activity_time { get; set; }
        public string activity_BP { get; set; }
        public string activity_contact { get; set; }
        public string activity_position { get; set; }
        public string activity_type { get; set; }
        public string activity_description { get; set; }
        public string activity_location { get; set; }
        public int activity_status { get; set; }
        public string activity_attach { get; set; }

        public DateTime? activity_createDate { get; set; }
        public DateTime? activity_modifDate { get; set; }
        public string activity_createBy { get; set; }
        public string activity_modifBy { get; set; }
    }
}