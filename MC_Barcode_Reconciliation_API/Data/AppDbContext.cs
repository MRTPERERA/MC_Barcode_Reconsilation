using MC_Barcode_Reconciliation_API.Models;
using Microsoft.EntityFrameworkCore;

namespace MC_Barcode_Reconciliation_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ScanBrcodePrint> ScanBrcodePrints { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanBrcodePrint>(entity =>
        {
            entity.ToTable("ScanBrcodePrint");
            entity.HasKey(e => e.RandomCode);

            entity.Property(e => e.RandomCode).HasColumnName("RandomCode").HasMaxLength(30);
            entity.Property(e => e.ProdShift).HasColumnName("ProdShit").HasMaxLength(10);
            entity.Property(e => e.PartNo).HasColumnName("Part_No").HasMaxLength(20);
            entity.Property(e => e.ShopOrder).HasColumnName("Shop_Order").HasMaxLength(10);
            entity.Property(e => e.DopId).HasColumnName("Dop_Id").HasMaxLength(10);
            entity.Property(e => e.EpfNo).HasColumnName("EPF_No").HasMaxLength(300);
            entity.Property(e => e.LoadingId).HasColumnName("LaodingID").HasMaxLength(50);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("User_Account");
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).HasColumnName("USERID").HasMaxLength(50);
            entity.Property(e => e.Password).HasColumnName("PASSWORD").HasMaxLength(50);
            entity.Property(e => e.Level).HasColumnName("LEVEL").HasMaxLength(10);
            entity.Property(e => e.Site).HasColumnName("SITE").HasMaxLength(3);
            entity.Property(e => e.SystemName).HasColumnName("SystemName").HasMaxLength(900);
            entity.Property(e => e.DefaultLocation).HasColumnName("DEFAULT_LOCATION").HasMaxLength(50);
            entity.Property(e => e.PlantLoc).HasColumnName("PLANT_LOC").HasMaxLength(50);
            entity.Property(e => e.IfsSite).HasColumnName("IFS_SITE").HasMaxLength(50);
            entity.Property(e => e.IfsSiteName).HasColumnName("IFS_SITE_NAME").HasMaxLength(50);
            entity.Property(e => e.RecevingCat).HasColumnName("RecevingCat").HasMaxLength(50);
        });
    }
}
