using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_cuti")]
    public class CutiModel
    {
        [Key]
        public string cuti_id { get; set; }   // varchar(20)

        public string kry_npk { get; set; }   // varchar(10)

        public string tipe_cuti { get; set; } // varchar(255)

        public string sub_tipe_cuti { get; set; } // varchar(255)

        public DateTime? mulai_dari { get; set; } // date

        public DateTime? sampai_dengan { get; set; } // date

        public int? durasi { get; set; } // int NULL

        public string status { get; set; } // varchar(255)

        public string alasan { get; set; } // varchar(255)

        public string lampiran { get; set; } // nvarchar(255)

        public DateTime? tanggal_pengajuan { get; set; } // date

        public DateTime? masa_berlaku_cuti { get; set; } // date

        public string jenis_cuti { get; set; } // varchar(255)

        public DateTime? tanggal_akhir { get; set; } // date

        public DateTime? tanggal_awal { get; set; } // date
    }
}
