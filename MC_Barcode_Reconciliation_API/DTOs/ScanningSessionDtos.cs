namespace MC_Barcode_Reconciliation_API.DTOs;

public class StartScanSessionRequestDto
{
    public string LoadingId { get; set; }
    public string UserId { get; set; }
}

public class ScanSessionDto
{
    public string SessionId { get; set; }
    public string LoadingId { get; set; }
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; }  // InProgress, Completed
    public int TotalScannedQty { get; set; }
    public int TotalPrintedQty { get; set; }
}

public class ScanBarcodeRequestDto
{
    public string SessionId { get; set; }
    public string RandomCode { get; set; }
    public int? ScannedQty { get; set; } = 1;
}

public class ScanBarcodeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ScanBarcodeDetailDto Barcode { get; set; }
}

public class ScanBarcodeDetailDto
{
    public string RandomCode { get; set; }
    public string PartNo { get; set; }
    public string Description { get; set; }
    public string Barcode { get; set; }
    public DateTime? ProdDate { get; set; }
    public string ProdShift { get; set; }
    public string Machine { get; set; }
    public string ShopOrder { get; set; }
    public string DopId { get; set; }
    public string ImagePath { get; set; }
}

public class PartSummaryItemDto
{
    public string PartNo { get; set; }
    public string Description { get; set; }
    public int ScannedQty { get; set; }
}

public class PartSummaryDto
{
    public string LoadingId { get; set; }
    public int TotalScannedQty { get; set; }
    public List<PartSummaryItemDto> Parts { get; set; }
    public int TotalParts { get; set; }
}

public class ReconciliationDto
{
    public string LoadingId { get; set; }
    public int LoadedQty { get; set; }
    public int ScannedQty { get; set; }
    public int BalanceQty { get; set; }
    public string Status { get; set; }  // COMPLETE, INCOMPLETE
    public decimal CompletionPercentage { get; set; }
}
