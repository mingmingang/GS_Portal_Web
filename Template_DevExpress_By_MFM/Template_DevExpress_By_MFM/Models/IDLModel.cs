using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_idl")]
    public class IDLModel
    {
        [Key]
        public int idl_id { get; set; } // int, primary key, identity

        [StringLength(255)]
        public string idl_no_request { get; set; } // varchar(255)

        [StringLength(10)]
        public string idl_npk { get; set; } // varchar(10)

        [StringLength(255)]
        public string idl_jenis_kegiatan { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_kategori_kendaraan { get; set; } // varchar(255)

        public DateTime? idl_waktu_berangkat { get; set; }

        public DateTime? idl_waktu_kembali { get; set; }

        [StringLength(255)]
        public string idl_lokasi_pertama { get; set; }

        [StringLength(255)]
        public string idl_lokasi_kedua { get; set; }

        [StringLength(255)]
        public string idl_lokasi_ketiga { get; set; }

        [StringLength(255)]
        public string idl_lokasi_aktual_pertama { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_lokasi_aktual_kedua { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_lokasi_aktual_ketiga { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_lokasi_akhir { get; set; } // varchar(255) - KOLOM BARU

        public DateTime? idl_waktu_lokasi_pertama { get; set; } // datetime - KOLOM BARU

        public DateTime? idl_waktu_lokasi_kedua { get; set; } // datetime - KOLOM BARU

        public DateTime? idl_waktu_lokasi_ketiga { get; set; } // datetime - KOLOM BARU

        public DateTime? idl_waktu_kembali_aktual { get; set; }

        [StringLength(255)]
        public string idl_keterangan { get; set; } // varchar(255)

        public string idl_berkas_lampiran { get; set; } // varchar(max)

        [StringLength(255)]
        public string idl_status { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_created_by { get; set; } // varchar(255)

        public DateTime? idl_created_date { get; set; } // datetime

        [StringLength(255)]
        public string idl_modif_by { get; set; } // varchar(255)

        public DateTime? idl_modif_date { get; set; } // datetime

        [StringLength(255)]
        public string idl_sopir { get; set; } // varchar(255)

        [StringLength(50)]
        public string idl_no_polisi { get; set; } // varchar(50)

        [StringLength(255)]
        public string idl_alasan_penolakan { get; set; } // varchar(255)

        [StringLength(255)]
        public string idl_alasan_pembatalan { get; set; } // varchar(255)
    }
}
