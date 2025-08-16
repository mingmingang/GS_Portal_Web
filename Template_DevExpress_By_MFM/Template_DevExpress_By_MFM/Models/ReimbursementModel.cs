// Models/ReimbursementModel.cs
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
        [Column("rbm_id")]
        public long RbmId { get; set; }

        [Column("kry_npk")]
        public string KryNpk { get; set; }

        [Column("rbm_tanggal_mulai")]
        public DateTime RbmTanggalMulai { get; set; }

        [Column("rbm_tanggal_selesai")]
        public DateTime? RbmTanggalSelesai { get; set; } // Nullable

        [Column("rbm_tipe")]
        public string RbmTipe { get; set; }

        [Column("org_id")]
        public long? OrgId { get; set; }

        [Column("dgs_id")]
        public string DgsId { get; set; }

        [Column("rs_id")]
        public int RsId { get; set; }

        [Column("rbm_cost")]
        public decimal? RbmCost { get; set; }

        [Column("rbm_status_submit")]
        public string RbmStatusSubmit { get; set; }

        [Column("rbm_alasan_pembatalan")]
        public string RbmAlasanPembatalan { get; set; }

        [Column("rbm_created_by")]
        public string RbmCreatedBy { get; set; }

        [Column("rbm_created_date")]
        public DateTime? RbmCreatedDate { get; set; }

        [Column("rbm_modify_by")]
        public string RbmModifyBy { get; set; }

        [Column("rbm_modify_date")]
        public DateTime? RbmModifyDate { get; set; }


        // Properti tambahan dari join tabel
        //[NotMapped]
        public string NamaKaryawan { get; set; }

        //[NotMapped]
        public string StatusKawin { get; set; }

        //[NotMapped]
        public string NamaDiagnosa { get; set; }

        //[NotMapped]
        public string NamaPasien { get; set; }

        //[NotMapped]
        public string HubunganPasien { get; set; }

        // Properti kalkulasi
        //[NotMapped]
        public string durasi { get; set; }
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
    }

    // Model untuk response final ke client
    public class ReimbursementLoadResult
    {
        public object data { get; set; }
        public int totalCount { get; set; }
        public ReimbursementSummary summary { get; set; }
    }
}