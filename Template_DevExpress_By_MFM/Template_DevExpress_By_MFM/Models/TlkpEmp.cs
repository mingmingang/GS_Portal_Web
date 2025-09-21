using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    // Nama tabel disesuaikan dengan DDL (Data Definition Language)
    [Table("tlkp_emp")]
    public class TlkpEmp
    {
        // Primary Key disesuaikan
        [Key]
        [Column("emp_npk")]
        public string EmpNpk { get; set; }

        [Column("plant")]
        public string Plant { get; set; }

        [Column("emp_nama")]
        public string EmpNama { get; set; }

        [Column("DepSeksi")]
        public string DepSeksi { get; set; }

        [Column("id_dept")]
        public string IdDept { get; set; }

        [Column("id_seksi")]
        public string IdSeksi { get; set; }

        [Column("NPKLeader")]
        public string NPKLeader { get; set; }

        [Column("no_hp")]
        public string NoHp { get; set; }

        [Column("emp_jabatan")]
        public string EmpJabatan { get; set; }

        [Column("emp_status_kawin")]
        public string EmpStatusKawin { get; set; }

        // Tipe int dibuat nullable (?) karena kolom di DB bisa NULL
        [Column("emp_golongan")]
        public int? EmpGolongan { get; set; }

        [Column("emp_created_by")]
        public string EmpCreatedBy { get; set; }

        // Tipe DateTime dibuat nullable (?) karena kolom di DB bisa NULL
        [Column("emp_created_date")]
        public DateTime? EmpCreatedDate { get; set; }

        [Column("emp_modif_by")]
        public string EmpModifBy { get; set; }

        // Tipe DateTime dibuat nullable (?) karena kolom di DB bisa NULL
        [Column("emp_modif_date")]
        public DateTime? EmpModifDate { get; set; }
    }
}