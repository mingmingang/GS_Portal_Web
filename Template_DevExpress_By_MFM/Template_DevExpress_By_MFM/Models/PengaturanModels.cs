using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_pengaturan")]
    public class PengaturanModels
    {
        [Key]
        [Column("pgtr_id")]
        public int PgtrId { get; set; }

        [Column("pgtr_code")]
        public string PgtrCode { get; set; }

        [Column("pgtr_nama")]
        public string PgtrNama { get; set; }

        [Column("pgtr_tipe")]
        public string PgtrTipe { get; set; }

        [Column("pgtr_desc")]
        public string PgtrDesc { get; set; }

        [Column("pgtr_IsRequired")]
        public bool PgtrIsRequired { get; set; }

        [Column("pgtr_status")]
        public int PgtrStatus { get; set; }

        [Column("pgtr_creaBy")]
        public string PgtrCreaBy { get; set; }

        [Column("pgtr_creadate")]
        public DateTime PgtrCreaDate { get; set; }

        [Column("pgtr_modiBy")]
        public string PgtrModiBy { get; set; }

        [Column("pgtr_modiDate")]
        public DateTime? PgtrModiDate { get; set; }
    }
}