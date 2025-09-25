using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstraTech.GsTrack.Models
{
    [Table("gs_track_cuti_jatah")]
    public class JatahCuti
    {
        [Key]
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("kry_npk")]
        public string KryNpk { get; set; }

        [Column("tahun")]
        public int Tahun { get; set; }

        [Column("hak_cuti")]
        public int HakCuti { get; set; }

        [Column("cuti_dipakai")]
        public int CutiDipakai { get; set; }

        [Column("cuti_sisa")]
        public int CutiSisa { get; set; }

        [Column("tipe_cuti")]
        public string TipeCuti { get; set; }

        [Column("masa_berlaku")]
        public string MasaBerlaku { get; set; }

        // Konstruktor tanpa parameter (diperlukan oleh Entity Framework)
        public JatahCuti()
        {
        }

        // Konstruktor dengan parameter
        public JatahCuti(int id, string kryNpk, int tahun, int hakCuti, int cutiDipakai, int cutiSisa, string tipeCuti, string masaBerlaku)
        {
            this.Id = id;
            this.KryNpk = kryNpk;
            this.Tahun = tahun;
            this.HakCuti = hakCuti;
            this.CutiDipakai = cutiDipakai;
            this.CutiSisa = cutiSisa;
            this.TipeCuti = tipeCuti;
            this.MasaBerlaku = masaBerlaku;
        }
    }
}