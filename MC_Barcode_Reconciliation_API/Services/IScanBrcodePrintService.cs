using MC_Barcode_Reconciliation_API.DTOs;

namespace MC_Barcode_Reconciliation_API.Services;

public interface IScanBrcodePrintService
{
    Task<IEnumerable<ScanBrcodePrintDto>> GetAllAsync(ScanBrcodePrintFilterDto filter);
    Task<ScanBrcodePrintDto?> GetByRandomCodeAsync(string randomCode);
    Task<ScanBrcodePrintDto?> UpdateAsync(string randomCode, UpdateScanBrcodePrintDto dto);
    Task<bool> ExistsAsync(string randomCode);
    Task<IEnumerable<ScanBrcodePrintDto>> GetByLoadingIdAsync(string laodingId);
    Task<IEnumerable<ScanBrcodePrintDto>> UpdateByLoadingIdAsync(string laodingId, BulkUpdateScanBrcodePrintDto dto);
}
