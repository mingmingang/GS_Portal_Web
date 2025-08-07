using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    [Table("t_idl")]
    public class ManageIDL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id_idl { get; set; }
        public string idl_npk { get; set; }
        public string id_nama { get; set; }
        public string idl_lokasikebrangkatan { get; set; }
        public int id_kegiatan { get; set; }
        public string idl_keterangan { get; set; }
        public DateTime? idl_tgl { get; set; }
        public string idl_plant { get; set; }
        public string idl_doc { get; set; }
    }

    public class ManageIDLTemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id_idl { get; set; }
        public string idl_attach { get; set; }
        public string idl_npk { get; set; }
        public string id_nama { get; set; }
        public string idl_BP { get; set; }
        public string idl_contact { get; set; }
        public string idl_position { get; set; }
        public string idl_lokasikebrangkatan { get; set; }
        public int id_kegiatan { get; set; }
        public string namakegiatan { get; set; }
        public string idl_keterangan { get; set; }
        public string activity_detail { get; set; }
        public DateTime? idl_tgl { get; set; }
        public string jam_inout { get; set; }
        public string jam_in { get; set; }
        public string jam_out { get; set; }
        public string idl_plant { get; set; }
        public string bitmap { get; set; }
        public string img_bitmap { get; set; }
        public string idl_doc { get; set; }

        public DateTime? idl_createDate { get; set; }
        public DateTime? idl_modifDate { get; set; }
        public string idl_createBy { get; set; }
        public string idl_modifBy { get; set; }
    }
    

    public class MappingManageIDL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 id_idl { get; set; }
        public string idl_npk { get; set; }
        public string id_nama { get; set; }
        public string idl_lokasikebrangkatan { get; set; }
        public int id_kegiatan { get; set; }
        public string namakegiatan { get; set; }
        public string idl_keterangan { get; set; }
        public DateTime? idl_tgl { get; set; }
        public string jam_inout { get; set; }
        public string jam_in { get; set; }
        public string jam_out { get; set; }
        public string idl_plant { get; set; }

    }

    [Table("tlkp_kegiatan_idl")]
    public class ListKegiatan
    {
        public int id { get; set; }
        public int status { get; set; }
        public string namakegiatan { get; set; }
    }
}