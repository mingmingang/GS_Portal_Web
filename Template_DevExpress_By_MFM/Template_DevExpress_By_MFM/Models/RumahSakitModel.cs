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
        public int RsId { get; set; }

        [Column("rs_nama")]
        public string RsNama { get; set; }

        [Column("rs_tipe")]
        public string RsTipe { get; set; }
    }
}