using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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

        [NotMapped]
        public int? PdfPageCountKwitansi { get; set; }

        [NotMapped]
        public int? PdfPageCountRincianObat { get; set; }

        [NotMapped]
        public int? PdfPageCountHasilLab { get; set; }

        [NotMapped]
        public int? PdfPageCountResumeMedis { get; set; }
    }

    // Model untuk menampung data ringkasan yang sudah dihitung
    //public class ReimbursementCardSummary
    //{
    //    public decimal Plafon { get; set; }
    //    public decimal Digunakan { get; set; }
    //    public decimal Sisa { get; set; }
    //    public decimal Unrealize { get; set; }
    //    public string Note { get; set; }
    //}

    public class DetailedReimbursementSummary
    {
        public ReimbursementCardSummary RawatJalan { get; set; }
        public ReimbursementCardSummary RawatInap { get; set; }
        public ReimbursementCardSummary Maternity { get; set; }
        public ReimbursementCardSummary KB { get; set; }
        public ReimbursementCardSummary Kacamata { get; set; }
    }

    public class ReimbursementLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }

        // [PERBAIKI TIPE DATA DI SINI]
        public DetailedReimbursementSummary summary { get; set; }
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

    #region Response Models for New UI
    public class PlafonSummaryResponse
    {
        [JsonProperty("collapsedSummary")]
        public ReimbursementCardSummary CollapsedSummary { get; set; }

        [JsonProperty("detailedSummary")]
        public DetailedReimbursementSummary DetailedSummary { get; set; }
    }

    public class ReimbursementCardSummary
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("plafon")]
        public decimal Plafon { get; set; }
        [JsonProperty("digunakan")]
        public decimal Digunakan { get; set; }
        [JsonProperty("sisa")]
        public decimal Sisa { get; set; }
    }
    // Catatan: Model ReimbursementLoadResult dihapus karena akan diganti dengan yang lebih spesifik.
    public class ReimbursementGridAndSummaryResult
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("summary")]
        public PlafonSummaryResponse Summary { get; set; }
    }
    #endregion

    public class ReimbursementViewModel
    {
        /// <summary>
        /// Tanggal karyawan dibuat, diformat sebagai string 'yyyy-MM-dd'.
        /// </summary>
        public string KryCreatedDate { get; set; }
        public string StatusPerkawinan { get; set; }
        public int Golongan { get; set; }
    }
}