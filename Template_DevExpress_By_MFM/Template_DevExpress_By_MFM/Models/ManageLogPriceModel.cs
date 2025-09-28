using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_log_history_price")]
    public class ManageLogPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id { get; set; }

        public Int32? cust_id { get; set; }
        public string log_part_number { get; set; }
        [MaxLength(50)]
        public string log_PN_old_jis { get; set; }
        [MaxLength(50)]
        public string log_PN_new_jis { get; set; }
        public string log_PN_batt_segmentation { get; set; }
        public Int32? log_periode_int { get; set; }
        public string log_periode { get; set; }
        public string log_year { get; set; }

        public Decimal? log_unit_price { get; set; }
        public int? log_status { get; set; }
        public DateTime? log_createDate { get; set; }
        public string log_createBy { get; set; }
    }


    //public class ListGroupManageOrder
    //{
    //    public string id_order { get; set; }
    //    public int status_order { get; set; }
    //}


    //public class StatusOrder {
    //    public int ID { get; set; }
    //    public string Name { get; set; }
    //}


    //[Table("manage_doc_order")]
    //public class ManageDocumentOrder
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long id_recnum { get; set; }
    //    public string id_order { get; set; }
    //    public string doc { get; set; }
    //}

}