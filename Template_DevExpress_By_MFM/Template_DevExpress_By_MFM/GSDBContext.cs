using Template_DevExpress_By_MFM.Models;
using System;
using System.Web;
using System.Data.Entity;

namespace Template_DevExpress_By_MFM
{
    public partial class GSDbContext : DbContext
    {
        public DbSet<ManageYearlyPlan> ManageYearlyPlan { get; set; }
        public DbSet<ManageBusinessPlan> ManageBusinessPlan { get; set; }
        public DbSet<ManageItemPartNumber> ManageItemPartNumber { get; set; }
        public DbSet<ManageForecast> ManageForecast { get; set; }
        public DbSet<ManageOrder> ManageOrder { get; set; }
        public DbSet<ManagePriceSimulation> ManagePriceSimulation { get; set; }
        public DbSet<ManagePriceSimulation_temp> ManagePriceSimulation_temp { get; set; }
        public DbSet<MasterPartNumber> MasterPartNumber { get; set; }
        public DbSet<MasterUser> MasterUser { get; set; }
        public DbSet<MasterType> MasterType { get; set; }
        public DbSet<MasterCountry> MasterCountry { get; set; }
        public DbSet<MasterCustomer> MasterCustomer { get; set; }
        //public DbSet<MasterType> MasterType { get; set; }
        public DbSet<ManageHistoryTransaction> ManageHistoryTransaction { get; set; }
        public DbSet<ManageDocumentOrder> ManageDocumentOrder { get; set; }
        public DbSet<ManageEmail> ManageEmail { get; set; }
        public DbSet<ManagePriceQuotation> ManagePriceQuotation { get; set; }
        public DbSet<ManageActivityMarketing> ManageActivityMarketing { get; set; }
        public DbSet<MasterLME> MasterLME { get; set; }
        public DbSet<MasterKurs> MasterKurs { get; set; }
        public DbSet<MasterBulan> MasterBulan { get; set; }
        public DbSet<MasterAttn> MasterAttn { get; set; }
        public DbSet<ManageLogPrice> ManageLogPrice { get; set; }

        public GSDbContext() : base("name=GSDbContext") { }

        public GSDbContext(string dbSource, string dbName, string dbUsers, string dbPass)
            : base($"Data Source=" + dbSource + ";initial catalog=" + dbName + ";User Id=" + dbUsers + ";Password=" + dbPass + "; ") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<GSDbContext>(null);
            base.OnModelCreating(modelBuilder);

            #region MANAGE YEARLY PLANE
            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_1)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_2)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_3)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_4)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_5)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_6)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_7)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_8)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_9)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_10)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_11)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_12)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageYearlyPlan>()
              .Property(p => p.qty_total)
              .HasPrecision(38, 18);
            #endregion

            #region MANAGE BUSINESS PLAN
            modelBuilder.Entity<ManageBusinessPlan>()
           .Property(p => p.qty_1)
           .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_2)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_3)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_4)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_5)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_6)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_7)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_8)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_9)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_10)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_11)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_12)
              .HasPrecision(38, 18);

            modelBuilder.Entity<ManageBusinessPlan>()
              .Property(p => p.qty_total)
              .HasPrecision(38, 18);
            #endregion

            #region MANAGE FORECAST
            modelBuilder.Entity<ManageForecast>()
           .Property(p => p.n2)
           .HasPrecision(38, 18);

            modelBuilder.Entity<ManageForecast>()
          .Property(p => p.n3)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageForecast>()
          .Property(p => p.n4)
          .HasPrecision(38, 18);
            #endregion

            #region MANAGE ORDER
            modelBuilder.Entity<ManageOrder>()
           .Property(p => p.total)
           .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.ship_to_JKT)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.ship_to_BDG)
          .HasPrecision(38, 18);  
            
            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.ship_to_SBY)
          .HasPrecision(38, 18); 
            
            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.ship_to_SMG)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
         .Property(p => p.confirm_to_JKT)
         .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.confirm_to_BDG)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.confirm_to_SBY)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.confirm_to_SMG)
          .HasPrecision(38, 18);

            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.confirm)
          .HasPrecision(38, 18);
            
            modelBuilder.Entity<ManageOrder>()
          .Property(p => p.adjustment)
          .HasPrecision(38, 18);
            #endregion
        }

    }

    public partial class GSDbContextGSTrack : DbContext
    {
        public DbSet<ManageIDL> ManageIDL { get; set; }

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
