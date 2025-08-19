using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_orang")]
    public class OrangModel
    {
        [Key]
        [Column("org_id")]
        public long OrgId { get; set; }

        [Column("org_nama")]
        public string OrgNama { get; set; }

        [Column("org_hubungan")]
        public string OrgHubungan { get; set; }

        [Column("kry_npk")]
        public string KryNpk { get; set; }
    }
}

//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Web;

//namespace Template_DevExpress_By_MFM.Models
//{
//    [Table("gs_track_orang")]
//    public class OrangModel
//    {
//        [Key]
//        public long OrgId { get; set; }

//        public string OrgNama { get; set; }

//        public string OrgHubungan { get; set; }

//        public string KryNpk { get; set; }
//    }
//}