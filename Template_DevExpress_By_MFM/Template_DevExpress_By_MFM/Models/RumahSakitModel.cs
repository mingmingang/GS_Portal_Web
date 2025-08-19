using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_rumah_sakit")]
    public class RumahSakitModel
    {
        [Key]
        [Column("rs_id")]
        public int rs_id { get; set; }

        [Column("rs_nama")]
        public string rs_nama { get; set; }
    }
}