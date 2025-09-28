using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_history_transaction")]
    public class ManageHistoryTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 id_recnum_history { get; set; }

        public string type_history { get; set; }

        public string desc_history { get; set; }

        public DateTime? datetime_history { get; set; }
    }



}