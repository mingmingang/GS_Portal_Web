using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("manage_order")]
    public class ManageOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 id_recnum_order { get; set; }

        [MaxLength(100)]
        public string id_order { get; set; }

        [MaxLength(20)]
        public string po_number { get; set; }
        public DateTime? order_date { get; set; }

        [MaxLength(100)]
        public string sales_order { get; set; }

        [MaxLength(100)]
        public string pn_customer { get; set; }

        public Int64 lot_size { get; set; }

        [MaxLength(100)]
        public string type_battery { get; set; }

        [MaxLength(100)]
        public string type_material { get; set; }

        [MaxLength(100)]
        public string brand { get; set; }

        [MaxLength(100)]
        public string group_order { get; set; }

        [MaxLength(100)]
        public string spec { get; set; }
        public int status_order { get; set; }
        public DateTime? insert_time { get; set; }
        public DateTime? update_time { get; set; }

        [MaxLength(50)]
        public string user_input { get; set; }

        [MaxLength(100)]
        public string pn_gs { get; set; }

        public Decimal ship_to_JKT { get; set; }

        public Decimal ship_to_BDG { get; set; }

        public Decimal ship_to_SBY { get; set; }

        public Decimal ship_to_SMG { get; set; }

        public Decimal confirm_to_JKT { get; set; }

        public Decimal confirm_to_BDG { get; set; }

        public Decimal confirm_to_SBY { get; set; }

        public Decimal confirm_to_SMG { get; set; }

        [MaxLength(50)]
        public string month_order { get; set; }

        [MaxLength(50)]
        public string year_order { get; set; }
        public Decimal total { get; set; }
        public Decimal confirm { get; set; }
        public Decimal adjustment { get; set; }
        public string file_excel { get; set; }
    }

    public class GetLastOrderID
    {
        public string id_order { get; set; }
    }

    public class ListGroupManageOrder
    {
        public string id_order { get; set; }
        public int status_order { get; set; }
    }

    public class ListHeaderManageOrder
    {
        public string id_order { get; set; }
        public string po_number { get; set; }
        public DateTime? order_date { get; set; }
        public string month_order { get; set; }
        public string year_order { get; set; }
        public Decimal ship_to_JKT { get; set; }
        public Decimal ship_to_BDG { get; set; }
        public Decimal ship_to_SBY { get; set; }
        public Decimal ship_to_SMG { get; set; }
        public Decimal total_po_ori { get; set; }
        public Decimal confirm { get; set; }
        public Decimal adjustment { get; set; }
        public Decimal total_po_agreed { get; set; }
        public int status_order { get; set; }
        public DateTime? insert_time { get; set; }
        public DateTime? update_time { get; set; }
    }

    public class StatusOrder {
        public int ID { get; set; }
        public string Name { get; set; }
    }


    [Table("manage_doc_order")]
    public class ManageDocumentOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id_recnum { get; set; }
        public string id_order { get; set; }
        public string doc { get; set; }
    }

}