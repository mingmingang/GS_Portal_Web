using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_country")]
    public class MasterCountry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 country_id { get; set; }
        public string country_name { get; set; }
        public DateTime? country_createDate { get; set; }
        public string country_createBy { get; set; }
        public DateTime? country_modifDate { get; set; }
        public string country_modifBy { get; set; }
        public int? country_status { get; set; }
    }



}