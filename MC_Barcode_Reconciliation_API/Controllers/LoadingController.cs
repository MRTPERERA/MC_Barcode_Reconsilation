using MC_Barcode_Reconciliation_API.DTOs;
using MC_Barcode_Reconciliation_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MC_Barcode_Reconciliation_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoadingController : ControllerBase
{
    private readonly IScanBrcodePrintService _barcodeService;

    public LoadingController(IScanBrcodePrintService barcodeService)
    {
        _barcodeService = barcodeService;
    }

    /// <summary>
    /// Get all barcodes for a specific loading batch.
    /// </summary>
    [HttpGet("{loadingId}/barcodes")]
    [ProducesResponseType(typeof(IEnumerable<ScanBrcodePrintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoadingBarcodes(string loadingId)
    {
        if (string.IsNullOrWhiteSpace(loadingId))
            return BadRequest(new { message = "LoadingId is required." });

        var filter = new ScanBrcodePrintFilterDto { LoadingId = loadingId, PageSize = 1000 };
        var records = await _barcodeService.GetAllAsync(filter);

        if (!records.Any())
            return NotFound(new { message = $"No barcodes found for LoadingId '{loadingId}'." });

        return Ok(records);
    }

    /// <summary>
    /// Get summary statistics for a loading batch.
    /// </summary>
    [HttpGet("{loadingId}/summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoadingSummary(string loadingId)
    {
        if (string.IsNullOrWhiteSpace(loadingId))
            return BadRequest(new { message = "LoadingId is required." });

        var filter = new ScanBrcodePrintFilterDto { LoadingId = loadingId, PageSize = 1000 };
        var records = await _barcodeService.GetAllAsync(filter);

        if (!records.Any())
            return NotFound(new { message = $"No barcodes found for LoadingId '{loadingId}'." });

        var totalPrintedQty = records.Sum(x => x.PrintedQty ?? 0);
        var totalScannedQty = records.Sum(x => x.ScnQty ?? 0);
        var uniqueParts = records.Select(x => x.PartNo).Distinct().Count();
        var uniqueSites = records.Select(x => x.SITE).Distinct().Count();

        return Ok(new
        {
            loadingId = loadingId,
            totalBarcodes = records.Count(),
            totalPrintedQty = totalPrintedQty,
            totalScannedQty = totalScannedQty,
            balanceQty = totalPrintedQty - totalScannedQty,
            uniqueParts = uniqueParts,
            uniqueSites = uniqueSites,
            completionPercentage = totalPrintedQty > 0 ? (totalScannedQty * 100m) / totalPrintedQty : 0
        });
    }

    /// <summary>
    /// Get part breakdown for a loading batch.
    /// </summary>
    [HttpGet("{loadingId}/parts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoadingParts(string loadingId)
    {
        if (string.IsNullOrWhiteSpace(loadingId))
            return BadRequest(new { message = "LoadingId is required." });

        var filter = new ScanBrcodePrintFilterDto { LoadingId = loadingId, PageSize = 1000 };
        var records = await _barcodeService.GetAllAsync(filter);

        if (!records.Any())
            return NotFound(new { message = $"No parts found for LoadingId '{loadingId}'." });

        var parts = records
            .GroupBy(x => new { x.PartNo, x.Description })
            .Select(g => new
            {
                partNo = g.Key.PartNo,
                description = g.Key.Description,
                count = g.Count(),
                totalPrintedQty = g.Sum(x => x.PrintedQty ?? 0),
                totalScannedQty = g.Sum(x => x.ScnQty ?? 0)
            })
            .OrderByDescending(x => x.count)
            .ToList();

        return Ok(new
        {
            loadingId = loadingId,
            totalParts = parts.Count,
            parts = parts
        });
    }
}
