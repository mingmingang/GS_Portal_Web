using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Template_DevExpress_By_MFM.Models
{
    // === MODEL ENTITAS UTAMA (DATABASE) ===
    [Table("t_reimburst_obat")]
    public class ReimbursementModel
    {
        [Key]
        [Column("rmb_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RmbId { get; set; }

        [Column("rmb_no_request")]
        public string RmbNoRequest { get; set; }

        [Column("rmb_npk")]
        public string RmbNpk { get; set; }

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

        [Column("rmb_nama_dokter")]
        public string RmbNamaDokter { get; set; }

        [Column("rmb_rumah_sakit")]
        public string RmbRumahSakit { get; set; }

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

        // Properti kalkulasi (Tidak dipetakan ke database)
        [NotMapped]
        public int StatusSortOrder { get; set; }
    }


    // === MODEL UNTUK API RESPONSE getReimbursementsAndSummary ===

    /// <summary>
    /// Model untuk menampilkan ringkasan plafon per tipe reimbursement.
    /// Bisa digunakan untuk ringkasan gabungan (collapsed) maupun rincian (detailed).
    /// </summary>
    public class ReimbursementCardSummary
    {
        public string Title { get; set; }
        public string Plafon { get; set; } // Menggunakan string untuk menampung angka atau "Sesuai Hak Gol."
        public decimal Digunakan { get; set; } // Tetap decimal untuk kalkulasi
        public string Sisa { get; set; } // Menggunakan string
        public string Note { get; set; }
    }

    /// <summary>
    /// Model yang membungkus ringkasan gabungan dan ringkasan detail per tipe.
    /// Ini adalah struktur utama untuk API getReimbursementSummary.
    /// </summary>
    public class PlafonSummaryResponse
    {
        public ReimbursementCardSummary CollapsedSummary { get; set; }
        // Menggunakan List agar tipe summary bisa dinamis sesuai data dari Sunfish
        public List<ReimbursementCardSummary> DetailedSummary { get; set; }
    }

    /// <summary>
    /// Model untuk hasil akhir API getReimbursementsAndSummary.
    /// Membungkus data list (grid) dan data summary.
    /// </summary>
    public class ReimbursementLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        // DIPERBAIKI: Menggunakan PlafonSummaryResponse yang baru dan dinamis
        public PlafonSummaryResponse summary { get; set; }
    }


    // === MODEL-MODEL LAIN YANG MUNGKIN DIPAKAI DI BAGIAN APLIKASI LAIN ===
    // (Model-model ini sudah dibersihkan dari duplikasi dan referensi yang rusak)

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

    public class ReimbursementViewModel
    {
        public string EmpId { get; set; }
        public string KryCreatedDate { get; set; }
        public string StatusPerkawinan { get; set; }
        public int Golongan { get; set; }
    }

    // === Model untuk Atasan/Manager View ===
    public class ReimbursementAtasanSummary
    {
        public int CountDisetujui { get; set; }
        public int CountDitolak { get; set; }
        public int CountMenunggu { get; set; }
        public int CountBelumVerifikasi { get; set; }
    }

    public class ReimbursementAtasanLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        public ReimbursementAtasanSummary summary { get; set; }
    }
}