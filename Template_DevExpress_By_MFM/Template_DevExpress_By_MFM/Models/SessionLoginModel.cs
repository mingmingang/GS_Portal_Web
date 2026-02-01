using System;
using System.Collections.Generic;

namespace Template_DevExpress_By_MFM.Models
{
    public class SessionLogin
    {
        public string empid { get; set; }
        public string npk { get; set; }
        public string fullname { get; set; }

        // --- Identitas Organisasi ---
        public string userplant { get; set; }     // J, K, S
        public string plant { get; set; }         // Sama dengan userplant

        public string userdepartment { get; set; } // Diisi dengan dept_name
        public string dept_code { get; set; }      // <-- Baru
        public int? dept_id { get; set; }          // <-- Baru

        // --- Identitas Jabatan & Role ---
        public string userjabatan { get; set; }    // Role Aktif (GA, Karyawan, Atasan)
        public string pos_name_id { get; set; }    // <-- Baru (Jabatan Asli Indo)
        public string pos_name_en { get; set; }    // <-- Baru (Jabatan Asli Inggris)

        public string selectedRole { get; set; }
        public List<string> availableRoles { get; set; }

        // --- Identitas Personal ---
        public DateTime? login_date { get; set; }
        public int? golongan { get; set; }
        public string statusKawin { get; set; }
        public DateTime? createdDate { get; set; }
        public int? company_id { get; set; }
        public string phone { get; set; }
        public string photo { get; set; }
        public int? pos_level { get; set; }

        // --- Tambahan jika ada field lain ---
        public int customer { get; set; }
        public string batt_category { get; set; }
        public string batt_segmentation { get; set; }
        public string periodic_price { get; set; }
        public int country { get; set; }
        public string userrole { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }
}