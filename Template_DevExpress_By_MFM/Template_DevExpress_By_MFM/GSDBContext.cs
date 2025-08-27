using Template_DevExpress_By_MFM.Models;
using System;
using System.Web;
using System.Data.Entity;

namespace Template_DevExpress_By_MFM
{
    public partial class GSDbContext : DbContext
    {

        public DbSet<ReimbursementModel> ReimbursementModel { get; set; }
        public DbSet<TlkpKaryawan> TlkpKaryawans { get; set; }

        public GSDbContext() : base("name=GSDbContext") { }

        public GSDbContext(string dbSource, string dbName, string dbUsers, string dbPass)
            : base($"Data Source=" + dbSource + ";initial catalog=" + dbName + ";User Id=" + dbUsers + ";Password=" + dbPass + "; ") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<GSDbContext>(null);
            base.OnModelCreating(modelBuilder);

         
        }

    }

    public partial class GSDbContextGSTrack : DbContext
    {
        public DbSet<ManageIDL> ManageIDL { get; set; }
        public DbSet<TlkpKaryawan> TlkpKaryawans { get; set; }

        public DbSet<CutiModel> gs_track_cuti { get; set; }
        public DbSet<ReimbursementModel> gs_track_reimbursement { get; set; }
        public DbSet<PengaturanModels> t_pengaturan { get; set; }
        public DbSet<OrangModel> gs_track_orang { get; set; }
        public DbSet<RumahSakitModel> gs_track_rumah_sakit { get; set; }
        public DbSet<DiagnosaModel> gs_track_diagnosa { get; set; }

        public GSDbContextGSTrack() : base("name=GSDbContextGSTrack") { }

        public GSDbContextGSTrack(string dbSource, string dbName, string dbUsers, string dbPass)
            : base($"Data Source=" + dbSource + ";initial catalog=" + dbName + ";User Id=" + dbUsers + ";Password=" + dbPass + "; ") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<GSDbContextGSTrack>(null);
            base.OnModelCreating(modelBuilder);
        }

    }
}
