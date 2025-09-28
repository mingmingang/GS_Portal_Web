using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_type_OEM")]
    public class MasterType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id { get; set; }
        public string item { get; set; }
        public string type_battery { get; set; }
        public string part_number_customer { get; set; }
        public DateTime? type_createDate { get; set; }
        public string type_createBy { get; set; }
        public DateTime? type_modifDate { get; set; }
        public string type_modifBy { get; set; }
        public int? type_status { get; set; }
    }

    public class ManageTypeOEM
    {
        public string item { get; set; }
        public string type_battery { get; set; }
        public string part_number_customer { get; set; }
    }

}