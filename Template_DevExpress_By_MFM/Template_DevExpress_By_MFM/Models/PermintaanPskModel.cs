using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_psk")]
    public class PermintaanPskModel
    {
        [Key]
        public string psk_id { get; set; }
        public string kry_npk { get; set; }
        public string psk_ap { get; set; }
        public string psk_ket { get; set; }
        public string psk_status { get; set; }
        public DateTime? psk_crea_date { get; set; }
        public string psk_crea_by { get; set; }
        public DateTime? psk_modi_date { get; set; }
        public string psk_modi_by { get; set; }
    }
}
