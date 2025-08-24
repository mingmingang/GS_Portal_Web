using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_reimbursement")]
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

        [Column("kry_npk")]
        public string KryNpk { get; set; }

        [Column("rmb_plant")]
        public string RmbPlant { get; set; }

        [Column("org_id")]
        public long? OrgId { get; set; }

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

        [Column("dgs_id")]
        public string DgsId { get; set; }

        [Column("rmb_diagnosa_other")]
        public string RmbDiagnosaOther { get; set; }

        [Column("rmb_nama_dokter")]
        public string RmbNamaDokter { get; set; }

        [Column("rs_id")]
        public int RsId { get; set; }

        [Column("rbm_file_path_kwitansi")]
        public string RbmFilePathKwitansi { get; set; }

        [Column("rbm_file_path_rincian_obat")]
        public string RbmFilePathRincianObat { get; set; }

        [Column("rbm_file_path_hasil_lab")]
        public string RbmFilePathHasilLab { get; set; }

        [Column("rbm_file_path_resume_medis")]
        public string RbmFilePathResumeMedis { get; set; }

        [Column("rmb_status")]
        public string RmbStatus { get; set; }

        [Column("rmb_alasan_penolakan")]
        public string RmbAlasanPenolakan { get; set; }

        [Column("rmb_alasan_pembatalan")]
        public string RmbAlasanPembatalan { get; set; }

        [Column("rmb_created_by")]
        public string RmbCreatedBy { get; set; }

        [Column("rmb_created_date")]
        public DateTime? RmbCreatedDate { get; set; }

        [Column("rmb_modif_by")]
        public string RmbModifBy { get; set; }

        [Column("rmb_modif_date")]
        public DateTime? RmbModifDate { get; set; }

        // Properti tambahan dari join tabel
        [NotMapped]
        public string NamaKaryawan { get; set; }

        [NotMapped]
        public string StatusKawin { get; set; }

        [NotMapped]
        public string NamaDiagnosa { get; set; }

        [NotMapped]
        public string NamaPasien { get; set; }

        [NotMapped]
        public string TipeRs {  get; set; }

        [NotMapped]
        public string NamaRumahSakit { get; set; }

        [NotMapped]
        public string HubunganPasien { get; set; }

        // Properti kalkulasi
        [NotMapped]
        public string durasi { get; set; }

        [NotMapped]
        public int StatusSortOrder { get; set; }
    }

    // Model untuk menampung data ringkasan yang sudah dihitung
    public class ReimbursementSummary
    {
        public decimal RawatJalanDigunakan { get; set; }
        public decimal RawatJalanUnrealize { get; set; }
        public decimal RawatInapDigunakan { get; set; }
        public decimal RawatInapUnrealize { get; set; }
        public decimal MaternityDigunakan { get; set; }
        public decimal MaternityUnrealize { get; set; }
        public decimal KbDigunakan { get; set; }
        public decimal KbUnrealize { get; set; }

        // Ringkasan (Summary) Plafon untuk Karyawan
        public decimal PlafonRawatJalan { get; set; }
        public string NoteRawatJalan { get; set; }

        public decimal PlafonRawatInap { get; set; } // Jika 0, dianggap unlimited
        public decimal PlafonMaternity { get; set; } // Jika 0, dianggap unlimited

        public decimal PlafonKb { get; set; }
        public string NoteKb { get; set; }
    }

    public class ReimbursementLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        public ReimbursementSummary summary { get; set; }
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
        /// <summary>
        /// ID dari pengajuan reimbursement yang akan dibatalkan.
        /// Wajib diisi.
        /// </summary>
        public int RmbId { get; set; } // Diubah ke int

        /// <summary>
        /// Alasan tertulis mengapa pengajuan ini dibatalkan.
        /// Wajib diisi.
        /// </summary>
        public string AlasanPembatalan { get; set; }
    }

    public class RejectRequestModel
    {
        public int RmbId { get; set; }
        public string AlasanPenolakan { get; set; }
    }

    public class ApproveRequestModel
    {
        public int RmbId { get; set; }
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