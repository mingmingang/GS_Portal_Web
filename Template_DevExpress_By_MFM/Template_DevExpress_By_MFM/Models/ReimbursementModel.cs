//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Template_DevExpress_By_MFM.Models
//{
//    [Table("gs_track_reimbursement")]
//    public partial class ReimbursementModel
//    {
//        [Key]
//        [Column("rbm_id")]
//        public long RbmId { get; set; }

//        [Required]
//        [StringLength(10)]
//        [Column("kry_npk")]
//        public string KryNpk { get; set; }

//        [Column("org_id")]
//        public long? OrgId { get; set; }

//        [Required]
//        [StringLength(50)]
//        [Column("rbm_tipe")]
//        public string RbmTipe { get; set; }

//        [Column("rbm_tanggal_mulai", TypeName = "date")]
//        public DateTime RbmTanggalMulai { get; set; }

//        [Column("rbm_tanggal_selesai", TypeName = "date")]
//        public DateTime? RbmTanggalSelesai { get; set; }

//        [Column("rbm_cost")]
//        public decimal RbmCost { get; set; }

//        [Required]
//        [StringLength(10)]
//        [Column("dgs_id")]
//        public string DgsId { get; set; }

//        [StringLength(255)]
//        [Column("rbm_diagnosa_other")]
//        public string RbmDiagnosaOther { get; set; }

//        [Column("rs_id")]
//        public int RsId { get; set; }

//        [Required]
//        [StringLength(255)]
//        [Column("rbm_dokter")]
//        public string RbmDokter { get; set; }

//        // Kolom untuk file (disimpan sebagai byte array)
//        [Column("rbm_file_path_kwitansi")]
//        public byte[] RbmFilePathKwitansi { get; set; }

//        [Column("rbm_file_path_rincian_obat")]
//        public byte[] RbmFilePathRincianObat { get; set; }

//        [Column("rbm_file_path_hasil_lab")]
//        public byte[] RbmFilePathHasilLab { get; set; }

//        [Column("rbm_file_path_resume_medis")]
//        public byte[] RbmFilePathResumeMedis { get; set; }

//        [Required]
//        [StringLength(50)]
//        [Column("rbm_status_submit")]
//        public string RbmStatusSubmit { get; set; }

//        [Required]
//        [Column("rbm_created_by")]
//        public string RbmCreatedBy { get; set; }

//        [Column("rbm_created_date")]
//        public DateTime RbmCreatedDate { get; set; }

//        [Column("rbm_modify_by")]
//        public string RbmModifyBy { get; set; }

//        [Column("rbm_modify_date")]
//        public DateTime? RbmModifyDate { get; set; }

//        [StringLength(255)]
//        [Column("rbm_alasan_pembatalan")]
//        public string RbmAlasanPembatalan { get; set; }

//        public int? durasi { get; set; }

//        // Navigation Properties (Untuk .Include())
//        [ForeignKey("KryNpk")]
//        public virtual TlkpKaryawan Karyawan { get; set; } // Sesuaikan dengan nama model Karyawan

//        [ForeignKey("OrgId")]
//        public virtual OrangModel Orang { get; set; } // Sudah ada

//        [ForeignKey("DgsId")]
//        public virtual DiagnosaModel Diagnosa { get; set; } // Sudah ada

//        [ForeignKey("RsId")]
//        public virtual RumahSakitModel RumahSakit { get; set; } // Sesuaikan dengan nama model Rumah Sakit

//        [NotMapped]
//        public string NamaPasienDisplay
//        {
//            get
//            {
//                // Jika data Orang (tanggungan) ada, gunakan namanya.
//                // Jika tidak, gunakan nama Karyawan.
//                return Orang?.org_nama ?? Karyawan?.kry_nama_karyawan ?? "N/A";
//            }
//        }

//        [NotMapped]
//        public string DurasiDisplay
//        {
//            get
//            {
//                // Jika tipe Rawat Inap atau Maternity dan tanggal selesai ada
//                if ((RbmTipe == "Rawat Inap" || RbmTipe == "Maternity") && RbmTanggalSelesai.HasValue)
//                {
//                    // Hitung selisih hari (+1 untuk inklusif)
//                    int days = (RbmTanggalSelesai.Value.Date - RbmTanggalMulai.Date).Days + 1;
//                    return $"{days} hari";
//                }
//                // Untuk tipe lain (Rawat Jalan, KB, dll.) durasi dianggap 1 hari
//                else
//                {
//                    return "1 hari";
//                }
//            }
//        }
//    }

//    // Base class/interface untuk summary agar bisa di-handle secara polimorfik
//    public interface ISummaryViewModel { }

//    // Summary untuk Atasan/HC
//    public class ManagerSummaryViewModel : ISummaryViewModel
//    {
//        public int DisetujuiCount { get; set; }
//        public int DitolakCount { get; set; }
//        public int MenungguPersetujuanCount { get; set; }
//        public int BelumDiverifikasiCount { get; set; }
//    }

//    // Summary untuk Karyawan
//    public class EmployeeSummaryViewModel : ISummaryViewModel
//    {
//        public decimal Plafon { get; set; }
//        public decimal Pemakaian { get; set; }
//        public decimal OnProgress { get; set; }
//        public decimal SisaPlafon { get; set; }
//    }

//    // ViewModel utama untuk View Index
//    public class ReimbursementIndexViewModel
//    {
//        public List<ReimbursementModel> Reimbursements { get; set; }
//        public ISummaryViewModel SummaryData { get; set; }
//        public List<int> AvailableYears { get; set; }
//        public int SelectedYear { get; set; }
//        public string UserRole { get; set; } // "Karyawan", "Atasan", "HC1", "HC2"

//        // Untuk Pagination
//        public int CurrentPage { get; set; }
//        public int PageSize { get; set; }
//        public int TotalRecords { get; set; }
//        public int TotalPages => (int)System.Math.Ceiling((double)TotalRecords / PageSize);
//    }
//}

// Models/ReimbursementModel.cs
using System;
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
        public long OrgId { get; set; }

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
        public string NamaKaryawan { get; set; }
        public string NamaDiagnosa { get; set; }
        public string NamaPasien { get; set; }
        public string HubunganPasien { get; set; }

        // Properti kalkulasi
        public string durasi { get; set; }
    }

    public class ReimbursementSummaryModel
    {
        public decimal Plafon { get; set; }
        public decimal Pemakaian { get; set; }
        public decimal OnProgress { get; set; }
        public decimal SisaPlafon { get; set; }
        public string PlafonDescription { get; set; } // Tambahan untuk memberi konteks
    }
}