using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_reimburst_obat")]
    public class ReimbursementModel
    {
        [Key]
        [Column("rmb_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RmbId { get; set; }

        [Column("rmb_no_request")]
        public string RmbNoRequest { get; set; }

        [Column("rmb_reim_from")]
        public string RmbReimFrom { get; set; }

        [Column("rmb_reim_for")]
        public string RmbReimFor { get; set; }

        [Column("rmb_npk")]
        public string RmbNpk { get; set; }

        [Column("rmb_plant")]
        public string RmbPlant { get; set; }

        [Column("rmb_nama_pasien")]
        public string RmbNamaPasien { get; set; }

        [Column("rmb_hubungan_pasien")]
        public string RmbHubunganPasien { get; set; }

        [Column("rmb_jenis_claim")]
        public string RmbJenisClaim { get; set; }

        [Column("rmb_tanggal_mulai")]
        public DateTime RmbTanggalMulai { get; set; }

        [Column("rmb_tanggal_akhir")]
        public DateTime? RmbTanggalAkhir { get; set; }

        [Column("rmb_biaya_periksa")]
        public decimal? RmbBiayaPeriksa { get; set; }

        [Column("rmb_biaya_diganti")]
        public decimal? RmbBiayaDiganti { get; set; }

        [Column("rmb_jenis_pembayaran")]
        public string RmbJenisPembayaran { get; set; }

        [Column("rmb_diagnosa")]
        public string RmbDiagnosa { get; set; }

        [Column("rmb_diagnosa_other")]
        public string RmbDiagnosaOther { get; set; }

        [Column("rmb_nama_dokter")]
        public string RmbNamaDokter { get; set; }

        [Column("rmb_tipe_rumah_sakit")]
        public string RmbTipeRumahSakit { get; set; }

        [Column("rmb_rumah_sakit")]
        public string RmbRumahSakit { get; set; }

        [Column("rmb_lampiran")]
        public string RmbLampiran { get; set; }

        [Column("rmb_status")]
        public string RmbStatus { get; set; }

        [Column("rmb_created_by")]
        public string RmbCreatedBy { get; set; }

        [Column("rmb_created_date")]
        public DateTime? RmbCreatedDate { get; set; }

        [Column("rmb_modif_by")]
        public string RmbModifBy { get; set; }

        [Column("rmb_modif_date")]
        public DateTime? RmbModifDate { get; set; }

        [Column("rmb_alasan_penolakan")]
        public string RmbAlasanPenolakan { get; set; }

        [Column("rmb_alasan_pembatalan")]
        public string RmbAlasanPembatalan { get; set; }

        [Column("rmb_file_path_kwitansi")]
        public string RmbFilePathKwitansi { get; set; }

        [Column("rmb_file_path_rincian_obat")]
        public string RmbFilePathRincianObat { get; set; }

        [Column("rmb_file_path_hasil_lab")]
        public string RmbFilePathHasilLab { get; set; }

        [Column("rmb_file_path_resume_medis")]
        public string RmbFilePathResumeMedis { get; set; }

        // Properti tambahan dari join tabel (Tidak dipetakan ke database)
        [NotMapped]
        public string NamaKaryawan { get; set; }

        [NotMapped]
        public string StatusKawin { get; set; }

        [NotMapped]
        public string NamaDiagnosa { get; set; }

        [NotMapped]
        public string NamaPasien { get; set; }

        [NotMapped]
        public string TipeRs { get; set; }

        [NotMapped]
        public string NamaRumahSakit { get; set; }

        [NotMapped]
        public string HubunganPasien { get; set; }

        // Properti kalkulasi (Tidak dipetakan ke database)
        [NotMapped]
        public string durasi { get; set; }

        [NotMapped]
        public int StatusSortOrder { get; set; }
    }

    // Model untuk menampung data ringkasan yang sudah dihitung
    public class ReimbursementSummary
    {
        // Hanya ada satu Plafon gabungan (dari Rawat Jalan)
        public decimal Plafon { get; set; }

        // Hanya ada satu nilai "Digunakan" yang di-SUM
        public decimal Digunakan { get; set; }

        // Sisa plafon (dihitung)
        public decimal Sisa { get; set; }

        // Catatan untuk menjelaskan plafon
        public string NotePlafon { get; set; }

        public decimal Unrealize { get; set; }
    }

    public class ReimbursementLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        public ReimbursementSummary summary { get; set; } // Pastikan ini menggunakan model summary yang baru
    }

    public class ReimbursementFormViewModel
    {
        public int GeneratedRmbId { get; set; }
        public string Npk { get; set; }
        public string NamaKaryawan { get; set; }
        public IEnumerable<object> PasienList { get; set; }
        public IEnumerable<object> DiagnosaList { get; set; }
    }

    public class CancelRequestModel
    {
        public int RmbId { get; set; }
        public string AlasanPembatalan { get; set; }
    }

    public class ReimbursementAtasanSummary
    {
        public int CountDisetujui { get; set; }
        public int CountDitolak { get; set; }
        public int CountMenunggu { get; set; }
        public int CountBelumVerifikasi { get; set; }
    }

    // Model untuk hasil response final, menggunakan summary yang baru
    public class ReimbursementAtasanLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        public ReimbursementAtasanSummary summary { get; set; }
    }
}