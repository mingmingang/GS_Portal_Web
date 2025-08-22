using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("gs_track_pic")]
    public class PermintaanPicModel
    {
        [Key]
        public string pic_id { get; set; }
        public string kry_npk { get; set; }
        public string pic_ah { get; set; }
        public string pic_status { get; set; }
        public DateTime? pic_crea_date { get; set; }
        public string pic_crea_by { get; set; }
        public DateTime? pic_modi_date { get; set; }
        public string pic_modi_by { get; set; }
    }
}
