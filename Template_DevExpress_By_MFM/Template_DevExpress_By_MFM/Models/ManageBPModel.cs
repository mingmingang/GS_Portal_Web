using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("dbo.ttccom1008888")]
    public class ManageBP
    {
        [Key]
        public string BPID { get; set; }
        public string display_name { get; set; }
        public string cust_name { get; set; }
        public string CCNT { get; set; }
        public string JOBT { get; set; }
        public string FULN { get; set; }
        public string CADR { get; set; }
        public string alamat2 { get; set; }
        public string alamat { get; set; }
    }

    [Table("dbo.ttcibd0018888")]
    public class ManagePN
    {
        public string item { get; set; }
        public string descrip { get; set; }
    }

    //[Table("tlkp_PN_OEM")]
    public class ManagePartNumberOEM
    {
        public Int32? id { get; set; }
        public string pn_gs { get; set; }
        public string pn_customer { get; set; }
        public string battery_type { get; set; }
        public string pn_bpid { get; set; }
        public Int32 pn_status { get; set; }
    }
}