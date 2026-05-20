using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MC_Barcode_Reconciliation_API.Models;

[Table("ScanBrcodePrint")]
public class ScanBrcodePrint
{
    [Key]
    [Column("RandomCode")]
    [MaxLength(30)]
    public string? RandomCode { get; set; }

    [Column("SerialNo")]
    public int? SerialNo { get; set; }

    [Column("Barcode")]
    [MaxLength(30)]
    public string? Barcode { get; set; }

    [Column("SITE")]
    [MaxLength(10)]
    public string? SITE { get; set; }

    [Column("ProdDate")]
    public DateTime? ProdDate { get; set; }

    // Note: column name in DB is "ProdShit" (typo for ProdShift)
    [Column("ProdShit")]
    [MaxLength(10)]
    public string? ProdShift { get; set; }

    [Column("Part_No")]
    [MaxLength(20)]
    public string? PartNo { get; set; }

    [Column("Description")]
    [MaxLength(900)]
    public string? Description { get; set; }

    [Column("Machine")]
    [MaxLength(8)]
    public string? Machine { get; set; }

    [Column("Shop_Order")]
    [MaxLength(10)]
    public string? ShopOrder { get; set; }

    [Column("Dop_Id")]
    [MaxLength(10)]
    public string? DopId { get; set; }

    // Stores 6-digit zero-padded EPF number e.g. "001234"
    [Column("EPF_No")]
    [MaxLength(300)]
    public string? EpfNo { get; set; }

    [Column("PrintedQty")]
    public int? PrintedQty { get; set; }

    [Column("PrintedDate")]
    public DateTime? PrintedDate { get; set; }

    [Column("ScnQty")]
    public int? ScnQty { get; set; }

    [Column("ScanDate")]
    public DateTime? ScanDate { get; set; }

    // Note: column name in DB is "LaodingID" (typo for LoadingID)
    [Column("LaodingID")]
    [MaxLength(50)]
    public string? LoadingId { get; set; }
}
