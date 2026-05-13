using MC_Barcode_Reconciliation_API.Models;
using Microsoft.EntityFrameworkCore;

namespace MC_Barcode_Reconciliation_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ScanBrcodePrint> ScanBrcodePrints { get; set; }

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
    }
}
