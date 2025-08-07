using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("manage_item_partnumber")]
    public class ManageItemPartNumber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 id_recnum_min { get; set; }

        [MaxLength(200)]
        public string pn_gs { get; set; }

        [MaxLength(200)]
        public string pn_customer { get; set; }
        public Int32 lot_size { get; set; }

        [MaxLength(100)]
        public string battery_type { get; set; }

        [MaxLength(100)]
        public string material_type { get; set; }

        [MaxLength(100)]
        public string brand { get; set; }

        [MaxLength(100)]
        public string group_item { get; set; }

        [MaxLength(100)]
        public string spec { get; set; }

        public DateTime? date_insert { get; set; }
        public DateTime? date_update { get; set; }
    }


    public class TemplateItemPartNumber_untukOrder
    {
        [Key]
        public string PN_CUSTOMER { get; set; }
        public string PN_GS { get; set; }
    }


}