using System;
using System.Collections.Generic;

namespace Template_DevExpress_By_MFM.Models
{
    public class SessionLogin
    {
        public string fullname { get; set; }
        public int customer { get; set; }
        public string batt_category { get; set; }
        public string batt_segmentation { get; set; }
        public string periodic_price { get; set; }
        public int country { get; set; }
        public string empid { get; set; }
        public string npk { get; set; }
        public string userrole { get; set; }
        public string userdepartment { get; set; }
        public string userplant { get; set; }  // Full name: "Jakarta", "Karawang", "Sunter"

        // BACKWARD COMPATIBILITY: Jabatan asli dari master data
        public string userjabatan { get; set; }  // "Karyawan", "Atasan", "HC"

        public string selectedRole { get; set; }
        public List<string> availableRoles { get; set; }

        public string plant { get; set; }      // Code: "J", "K", "S"
        public DateTime? login_date { get; set; }
        public int? golongan { get; set; }
        public string statusKawin { get; set; }
        public DateTime? createdDate { get; set; }
        public int? company_id { get; set; }
        public string phone { get; set; }
        public string photo { get; set; }
        public int? pos_level { get; set; }
    }
}