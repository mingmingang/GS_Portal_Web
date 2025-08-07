using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Template_DevExpress_By_MFM.Models
{
    public class CutiModel
    {
        public string NoPengajuan { get; set; }
        public string Permohonan { get; set; }
        public string TipeCuti { get; set; }
        public DateTime TanggalCuti { get; set; }
        public string Durasi { get; set; }
        public string Status { get; set; }
        public string Pembatalan { get; set; }
        public DateTime TanggalMulai { get; set; }
        public DateTime TanggalSelesai { get; set; }
        public string Keterangan { get; set; }

        public string RangeTanggalCuti { get; set; }
        public List<DateTime> TanggalList { get; set; }
        public string Alasan { get; set; }

    }
}