using MC_Barcode_Reconciliation_API.DTOs;

namespace MC_Barcode_Reconciliation_API.Services;

public interface IScanningService
{
    Task<ScanSessionDto?> StartSessionAsync(StartScanSessionRequestDto request);
    Task<ScanSessionDto?> GetSessionAsync(string sessionId);
    Task<ScanBarcodeResponseDto> ScanBarcodeAsync(ScanBarcodeRequestDto request);
    Task<List<ScanBarcodeDetailDto>> GetScannedItemsAsync(string sessionId);
    Task<PartSummaryDto> GetPartSummaryAsync(string sessionId);
    Task<ReconciliationDto> GetReconciliationAsync(string sessionId);
    Task<bool> CompleteSessionAsync(string sessionId);
    Task<bool> CancelSessionAsync(string sessionId);
}
