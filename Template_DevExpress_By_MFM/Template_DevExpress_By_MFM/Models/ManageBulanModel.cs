using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_bulan")]
    public class MasterBulan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id { get; set; }
        public string bulan { get; set; }
        public string bln { get; set; }
        public int? bulan_status { get; set; }
    }
}