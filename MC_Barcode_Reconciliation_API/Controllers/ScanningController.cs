using MC_Barcode_Reconciliation_API.DTOs;
using MC_Barcode_Reconciliation_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MC_Barcode_Reconciliation_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScanningController : ControllerBase
{
    private readonly IScanningService _service;

    public ScanningController(IScanningService service)
    {
        _service = service;
    }

    /// <summary>
    /// Start a new barcode scanning session for a loading batch.
    /// </summary>
    [HttpPost("sessions/start")]
    [ProducesResponseType(typeof(ScanSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartSession([FromBody] StartScanSessionRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = await _service.StartSessionAsync(request);
        if (session == null)
            return NotFound(new { message = "Loading or User not found." });

        return Ok(session);
    }

    /// <summary>
    /// Get current session details and scanned count.
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(ScanSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        var session = await _service.GetSessionAsync(sessionId);
        if (session == null)
            return NotFound(new { message = $"Session '{sessionId}' not found." });

        return Ok(session);
    }

    /// <summary>
    /// Scan a barcode and validate it belongs to the loading batch.
    /// Returns barcode details if successful.
    /// </summary>
    [HttpPost("sessions/{sessionId}/scan-barcode")]
    [ProducesResponseType(typeof(ScanBarcodeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ScanBarcode(string sessionId, [FromBody] ScanBarcodeRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Inject session ID from URL
        request.SessionId = sessionId;

        var response = await _service.ScanBarcodeAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get all barcodes scanned in this session.
    /// </summary>
    [HttpGet("sessions/{sessionId}/scanned-items")]
    [ProducesResponseType(typeof(List<ScanBarcodeDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScannedItems(string sessionId)
    {
        var items = await _service.GetScannedItemsAsync(sessionId);
        return Ok(items);
    }

    /// <summary>
    /// Get summary of parts scanned in this session.
    /// Grouped by Part_No with quantities.
    /// </summary>
    [HttpGet("sessions/{sessionId}/summary")]
    [ProducesResponseType(typeof(PartSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartSummary(string sessionId)
    {
        var summary = await _service.GetPartSummaryAsync(sessionId);
        if (summary == null)
            return NotFound(new { message = "No summary data for session." });

        return Ok(summary);
    }

    /// <summary>
    /// Get reconciliation report (Loaded vs Scanned vs Balance).
    /// </summary>
    [HttpGet("sessions/{sessionId}/reconciliation")]
    [ProducesResponseType(typeof(ReconciliationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReconciliation(string sessionId)
    {
        var recon = await _service.GetReconciliationAsync(sessionId);
        if (recon == null)
            return NotFound(new { message = "No reconciliation data for session." });

        return Ok(recon);
    }

    /// <summary>
    /// Complete the scanning session (finalize all scanned quantities).
    /// </summary>
    [HttpPut("sessions/{sessionId}/complete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteSession(string sessionId)
    {
        var result = await _service.CompleteSessionAsync(sessionId);
        if (!result)
            return NotFound(new { message = "Session not found." });

        return Ok(new { message = "Session completed successfully.", sessionId });
    }

    /// <summary>
    /// Cancel the scanning session (discard all scans).
    /// </summary>
    [HttpPut("sessions/{sessionId}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSession(string sessionId)
    {
        var result = await _service.CancelSessionAsync(sessionId);
        if (!result)
            return NotFound(new { message = "Session not found." });

        return Ok(new { message = "Session cancelled. Scans discarded.", sessionId });
    }
}
