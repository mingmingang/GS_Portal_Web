using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Template_DevExpress_By_MFM.Models
{
    // === MODEL ENTITAS UTAMA (DATABASE) ===
    [Table("t_pengajuan_surat_jaminan")]
    public class SuratJaminanModel
    {
        [Key]
        [Column("psj_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PsjId { get; set; }

        [Column("psj_no_request_record")]
        public string PsjNoRequestRecord { get; set; }

        [Column("psj_npk")]
        public string PsjNpk { get; set; }

        [Column("psj_plant")]
        public string PsjPlant { get; set; }

        [Column("psj_tanggal_periksa")]
        public DateTime? PsjTanggalPeriksa { get; set; }

        [Column("psj_rumah_sakit")]
        public string PsjRumahSakit { get; set; }

        [Column("psj_tipe_jaminan")]
        public string PsjTipeJaminan { get; set; }

        [Column("psj_pasien")]
        public string PsjPasien { get; set; }

        [Column("psj_keterangan")]
        public string PsjKeterangan { get; set; }

        [Column("psj_status")]
        public string PsjStatus { get; set; }

        [Column("psj_created_by")]
        public string PsjCreatedBy { get; set; }

        [Column("psj_created_date")]
        public DateTime? PsjCreatedDate { get; set; }

        [Column("psj_modif_by")]
        public string PsjModifBy { get; set; }

        [Column("psj_modif_date")]
        public DateTime? PsjModifDate { get; set; }

        [Column("psj_alasan_penolakan")]
        public string PsjAlasanPenolakan { get; set; }

        // Properti tambahan dari join tabel (Tidak dipetakan ke database)
        [NotMapped]
        public string NamaKaryawan { get; set; }

        [NotMapped]
        public string StatusKawin { get; set; }

        // Properti kalkulasi (Tidak dipetakan ke database)
        [NotMapped]
        public int StatusSortOrder { get; set; }
    }


    // === MODEL-MODEL LAIN YANG MUNGKIN DIPAKAI DI BAGIAN APLIKASI LAIN ===
    // (Model-model ini sudah dibersihkan dari duplikasi dan referensi yang rusak)

    public class SuratJaminanFormViewModel
    {
        public int GeneratedPsjId { get; set; }
        public string Npk { get; set; }
        public string NamaKaryawan { get; set; }
        public IEnumerable<object> PasienList { get; set; }
    }


    public class SuratJaminanViewModel
    {
        public string EmpId { get; set; }
        public string EmpNo { get; set; }
        public string KryCreatedDate { get; set; }
        public string StatusPerkawinan { get; set; }
        public int Golongan { get; set; }
        public string Plant {  get; set; }
    }


}