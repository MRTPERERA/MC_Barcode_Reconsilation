using MC_Barcode_Reconciliation_API.Data;
using MC_Barcode_Reconciliation_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MC_Barcode_Reconciliation_API.Services;

public class ScanningService : IScanningService
{
    private readonly AppDbContext _context;

    public ScanningService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ScanSessionDto?> StartSessionAsync(StartScanSessionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.LoadingId) || string.IsNullOrWhiteSpace(request?.UserId))
            return null;

        // Verify loading exists
        var loadingExists = await _context.ScanBrcodePrints
            .AnyAsync(x => x.LoadingId == request.LoadingId);

        if (!loadingExists)
            return null;

        // Verify user exists
        var userExists = await _context.UserAccounts
            .AnyAsync(u => u.UserId == request.UserId);

        if (!userExists)
            return null;

        var sessionId = Guid.NewGuid().ToString();
        var totalPrintedQty = await _context.ScanBrcodePrints
            .Where(x => x.LoadingId == request.LoadingId)
            .SumAsync(x => x.PrintedQty ?? 0);

        return new ScanSessionDto
        {
            SessionId = sessionId,
            LoadingId = request.LoadingId,
            UserId = request.UserId,
            StartTime = DateTime.UtcNow,
            Status = "InProgress",
            TotalScannedQty = 0,
            TotalPrintedQty = totalPrintedQty
        };
    }

    public async Task<ScanSessionDto?> GetSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        var scannedQty = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId && x.IsScannedInSession == true)
            .SumAsync(x => x.ScannedQtyInSession ?? 0);

        // Get session info from first matching barcode
        var sessionInfo = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId)
            .Select(x => new { x.LoadingId, x.ScanSessionId })
            .FirstOrDefaultAsync();

        if (sessionInfo == null)
            return null;

        var totalPrintedQty = await _context.ScanBrcodePrints
            .Where(x => x.LoadingId == sessionInfo.LoadingId)
            .SumAsync(x => x.PrintedQty ?? 0);

        return new ScanSessionDto
        {
            SessionId = sessionId,
            LoadingId = sessionInfo.LoadingId,
            UserId = "", // Would need to store in DB
            StartTime = DateTime.UtcNow,
            Status = "InProgress",
            TotalScannedQty = scannedQty,
            TotalPrintedQty = totalPrintedQty
        };
    }

    public async Task<ScanBarcodeResponseDto> ScanBarcodeAsync(ScanBarcodeRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.SessionId) || string.IsNullOrWhiteSpace(request?.RandomCode))
        {
            return new ScanBarcodeResponseDto
            {
                Success = false,
                Message = "SessionId and RandomCode required."
            };
        }

        // Find barcode
        var barcode = await _context.ScanBrcodePrints
            .FirstOrDefaultAsync(x => x.RandomCode == request.RandomCode);

        if (barcode == null)
        {
            return new ScanBarcodeResponseDto
            {
                Success = false,
                Message = $"Barcode {request.RandomCode} not found."
            };
        }

        // Get the loading session to verify it matches
        var sessionBarcode = await _context.ScanBrcodePrints
            .FirstOrDefaultAsync(x => x.ScanSessionId == request.SessionId);

        if (sessionBarcode == null)
        {
            return new ScanBarcodeResponseDto
            {
                Success = false,
                Message = "Session not found."
            };
        }

        // Verify barcode belongs to this loading
        if (barcode.LoadingId != sessionBarcode.LoadingId)
        {
            return new ScanBarcodeResponseDto
            {
                Success = false,
                Message = $"Barcode belongs to LoadingID {barcode.LoadingId}, not {sessionBarcode.LoadingId}"
            };
        }

        // Check if already scanned in this session
        if (barcode.IsScannedInSession == true && barcode.ScanSessionId == request.SessionId)
        {
            return new ScanBarcodeResponseDto
            {
                Success = false,
                Message = "Barcode already scanned in this session."
            };
        }

        // Mark as scanned
        barcode.ScanSessionId = request.SessionId;
        barcode.IsScannedInSession = true;
        barcode.ScannedQtyInSession = request.ScannedQty ?? 1;
        barcode.ScannedTimeSession = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ScanBarcodeResponseDto
        {
            Success = true,
            Message = "Barcode scanned successfully.",
            Barcode = new ScanBarcodeDetailDto
            {
                RandomCode = barcode.RandomCode,
                PartNo = barcode.PartNo,
                Description = barcode.Description,
                Barcode = barcode.Barcode,
                ProdDate = barcode.ProdDate,
                ProdShift = barcode.ProdShift,
                Machine = barcode.Machine,
                ShopOrder = barcode.ShopOrder,
                DopId = barcode.DopId,
                ImagePath = $"/images/parts/{barcode.PartNo}.jpg"
            }
        };
    }

    public async Task<List<ScanBarcodeDetailDto>> GetScannedItemsAsync(string sessionId)
    {
        return await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId && x.IsScannedInSession == true)
            .Select(x => new ScanBarcodeDetailDto
            {
                RandomCode = x.RandomCode,
                PartNo = x.PartNo,
                Description = x.Description,
                Barcode = x.Barcode,
                ProdDate = x.ProdDate,
                ProdShift = x.ProdShift,
                Machine = x.Machine,
                ShopOrder = x.ShopOrder,
                DopId = x.DopId,
                ImagePath = $"/images/parts/{x.PartNo}.jpg"
            })
            .ToListAsync();
    }

    public async Task<PartSummaryDto> GetPartSummaryAsync(string sessionId)
    {
        var items = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId && x.IsScannedInSession == true)
            .GroupBy(x => new { x.PartNo, x.Description })
            .Select(g => new PartSummaryItemDto
            {
                PartNo = g.Key.PartNo,
                Description = g.Key.Description,
                ScannedQty = g.Sum(x => x.ScannedQtyInSession ?? 1)
            })
            .ToListAsync();

        var loadingId = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId)
            .Select(x => x.LoadingId)
            .FirstOrDefaultAsync();

        return new PartSummaryDto
        {
            LoadingId = loadingId,
            TotalScannedQty = items.Sum(x => x.ScannedQty),
            Parts = items,
            TotalParts = items.Count
        };
    }

    public async Task<ReconciliationDto> GetReconciliationAsync(string sessionId)
    {
        var loadingId = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId)
            .Select(x => x.LoadingId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(loadingId))
            return null;

        var loadedQty = await _context.ScanBrcodePrints
            .Where(x => x.LoadingId == loadingId)
            .SumAsync(x => x.PrintedQty ?? 0);

        var scannedQty = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId && x.IsScannedInSession == true)
            .SumAsync(x => x.ScannedQtyInSession ?? 1);

        var balanceQty = loadedQty - scannedQty;
        var isComplete = balanceQty == 0;
        var percentage = loadedQty > 0 ? (scannedQty * 100m) / loadedQty : 0;

        return new ReconciliationDto
        {
            LoadingId = loadingId,
            LoadedQty = loadedQty,
            ScannedQty = scannedQty,
            BalanceQty = balanceQty,
            Status = isComplete ? "COMPLETE" : "INCOMPLETE",
            CompletionPercentage = percentage
        };
    }

    public async Task<bool> CompleteSessionAsync(string sessionId)
    {
        var barcodes = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId)
            .ToListAsync();

        foreach (var barcode in barcodes)
        {
            barcode.ScnQty = barcode.ScannedQtyInSession;
            barcode.ScanDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelSessionAsync(string sessionId)
    {
        var barcodes = await _context.ScanBrcodePrints
            .Where(x => x.ScanSessionId == sessionId)
            .ToListAsync();

        foreach (var barcode in barcodes)
        {
            barcode.ScanSessionId = null;
            barcode.IsScannedInSession = false;
            barcode.ScannedQtyInSession = null;
            barcode.ScannedTimeSession = null;
            barcode.ScannedByUserId = null;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
