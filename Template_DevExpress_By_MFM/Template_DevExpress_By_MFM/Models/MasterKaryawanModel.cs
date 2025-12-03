using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("tlkp_karyawan")]
    public class MasterKaryawan
    {
        [Key]
        public string kry_npk { get; set; }
        public string kry_nama_karyawan { get; set; }
        public string kry_foto_karyawan { get; set; }
        public string kry_plant { get; set; }
        public string kry_departemen { get; set; }
        public string kry_email { get; set; }
        public string kry_no_handphone { get; set; }
        public DateTime? kry_tanggal_lahir { get; set; }
        public int? kry_status { get; set; }
        public string kry_created_by { get; set; }
        public DateTime? kry_created_date { get; set; }
        public string kry_modif_by { get; set; }
        public DateTime? kry_modif_date { get; set; }
        public string kry_password { get; set; }
        public string kry_jabatan { get; set; }
        public string kry_status_kawin { get; set; }
        public string kry_golongan { get; set; }
        public string kry_alamat { get; set; }
        public decimal? jumlah_plafon { get; set; }
        public decimal? penggunaan_plafon { get; set; }
    }
}
