using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_imp")]
    public class IMPModel
    {
        [Key]
        public int imp_id { get; set; } // int, primary key, identity

        [StringLength(255)]
        public string imp_no_request { get; set; } // varchar(255)

        [StringLength(10)]
        public string imp_npk { get; set; } // varchar(10)

        [StringLength(255)]
        public string imp_jenis_kegiatan { get; set; } // varchar(255)

        [StringLength(255)]
        public string imp_waktu_izin { get; set; } // varchar(255)

        public DateTime? imp_tanggal_berangkat { get; set; } // date

        public TimeSpan? imp_waktu_berangkat { get; set; } // time(7)

        public DateTime? imp_tanggal_kembali { get; set; } // date

        public TimeSpan? imp_waktu_kembali { get; set; } // time(7)

        [StringLength(255)]
        public string imp_keterangan { get; set; } // varchar(255)

        public string imp_berkas_lampiran { get; set; } // varchar(max)

        [StringLength(255)]
        public string imp_status { get; set; } // varchar(255)

        [StringLength(255)]
        public string imp_created_by { get; set; } // varchar(255)

        public DateTime? imp_created_date { get; set; } // datetime

        [StringLength(255)]
        public string imp_modif_by { get; set; } // varchar(255)

        public DateTime? imp_modif_date { get; set; } // datetime

        public DateTime? imp_berangkat_aktual { get; set; } // datetime

        public DateTime? imp_kembali_aktual { get; set; } // datetime

        [StringLength(255)]
        public string imp_alasan_penolakan { get; set; } // varchar(255)

        [StringLength(255)]
        public string imp_alasan_pembatalan { get; set; } // varchar(255)
    }
}
