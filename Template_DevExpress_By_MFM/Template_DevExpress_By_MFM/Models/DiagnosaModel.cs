using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_diagnosa")]
    public class DiagnosaModel
    {
        [Key]
        [Column("dgs_id")]
        public string DgsId { get; set; }

        [Column("dgs_nama")]
        public string DgsNama { get; set; }
    }
}