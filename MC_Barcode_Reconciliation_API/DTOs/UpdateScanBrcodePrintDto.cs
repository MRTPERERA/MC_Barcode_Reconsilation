namespace MC_Barcode_Reconciliation_API.DTOs;

public class UpdateScanBrcodePrintDto
{
    public int? SerialNo { get; set; }
    public string? Barcode { get; set; }
    public string? SITE { get; set; }
    public DateTime? ProdDate { get; set; }
    public string? ProdShift { get; set; }
    public string? PartNo { get; set; }
    public string? Description { get; set; }
    public string? Machine { get; set; }
    public string? ShopOrder { get; set; }
    public string? DopId { get; set; }
    public string? EpfNo { get; set; }
    public int? PrintedQty { get; set; }
    public DateTime? PrintedDate { get; set; }
    public int? ScnQty { get; set; }
    public DateTime? ScanDate { get; set; }
    public string? LoadingId { get; set; }
}
