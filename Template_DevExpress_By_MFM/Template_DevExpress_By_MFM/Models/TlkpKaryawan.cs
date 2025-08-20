using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_karyawan")]
    public class TlkpKaryawan
    {
        [Key]
        [Column("kry_npk")]
        public string kry_npk { get; set; }

        [Column("kry_password")]
        public string kry_password { get; set; }

        [Column("kry_status")]
        public string kry_status { get; set; }

        [Column("kry_jabatan")]
        public string kry_jabatan { get; set; }

        [Column("kry_nama_karyawan")]
        public string kry_nama_karyawan { get; set; }

        [Column("kry_golongan")]
        public int kry_golongan { get; set; }

        [Column("kry_status_kawin")]
        public string kry_status_kawin { get; set; }

        [Column("kry_plant")]
        public string kry_plant { get; set; }

        [Column("kry_departemen")]
        public string kry_departemen { get; set; }

        [Column("kry_created_date")]
        public DateTime? kry_created_date { get; set; }

        // Tambahkan properti lain dari tabel tlkp_karyawan jika diperlukan
    }
}