namespace MC_Barcode_Reconciliation_API.DTOs;

public class ScanBrcodePrintFilterDto
{
    public string? Site { get; set; }
    public string? PartNo { get; set; }
    public string? Machine { get; set; }
    public string? ShopOrder { get; set; }
    public string? LoadingId { get; set; }
    public DateTime? ProdDateFrom { get; set; }
    public DateTime? ProdDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
