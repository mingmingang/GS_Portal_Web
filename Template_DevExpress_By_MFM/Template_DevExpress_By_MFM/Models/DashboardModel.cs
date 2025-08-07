using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template_DevExpress_By_MFM.Models
{
    public class DashboardCardModel
    {
        [Key]
        public string tahun { get; set; }
        //public Int64 total { get; set; }
        public Decimal total { get; set; }

    }

    public class DashboardBarModel
    {
        [Key]
        public Int64 id_recnum_mav { get; set; }
        public string nama_vm { get; set; }
        public Decimal total { get; set; }

    }

    public class DashboardPieModel
    {
        public string nama_variant { get; set; }
        //public Int32 total { get; set; }
        public Decimal total { get; set; }
    }

    public class DashboardLineHeaderModel
    {
        public string tanggal { get; set; }
    }
    public class DashboardLineQtyModel
    {
        public Decimal total { get; set; }
    }

    public class DashboardChartModel
    {
        public string transaksi { get; set; }
        public string tahun { get; set; }
        public string bulan { get; set; }
        public int urutanbulan { get; set; }
        //public Int64 total { get; set; }
        public Decimal total { get; set; }
    }

    public class DashboardTempChartYBModel
    {
        public string transaksi { get; set; }
        public string tahun { get; set; }
        public Decimal januari { get; set; }
        public Decimal februari { get; set; }
        public Decimal maret { get; set; }
        public Decimal april { get; set; }
        public Decimal mei { get; set; }
        public Decimal juni { get; set; }
        public Decimal juli { get; set; }
        public Decimal agustus { get; set; }
        public Decimal september { get; set; }
        public Decimal oktober { get; set; }
        public Decimal november { get; set; }
        public Decimal desember { get; set; }
    }

}