using MC_Barcode_Reconciliation_API.DTOs;
using MC_Barcode_Reconciliation_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MC_Barcode_Reconciliation_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScanBarcodePrintController : ControllerBase
{
    private readonly IScanBrcodePrintService _service;

    public ScanBarcodePrintController(IScanBrcodePrintService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all ScanBrcodePrint records with optional filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScanBrcodePrintDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ScanBrcodePrintFilterDto filter)
    {
        var records = await _service.GetAllAsync(filter);
        return Ok(records);
    }

    /// <summary>
    /// Get a single ScanBrcodePrint record by RandomCode.
    /// </summary>
    [HttpGet("{randomCode}")]
    [ProducesResponseType(typeof(ScanBrcodePrintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRandomCode(string randomCode)
    {
        var record = await _service.GetByRandomCodeAsync(randomCode);
        if (record is null)
            return NotFound(new { message = $"Record with RandomCode '{randomCode}' not found." });

        return Ok(record);
    }

    /// <summary>
    /// Update a ScanBrcodePrint record by RandomCode. Only provided fields are updated.
    /// </summary>
    [HttpPut("{randomCode}")]
    [ProducesResponseType(typeof(ScanBrcodePrintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string randomCode, [FromBody] UpdateScanBrcodePrintDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _service.UpdateAsync(randomCode, dto);
        if (updated is null)
            return NotFound(new { message = $"Record with RandomCode '{randomCode}' not found." });

        return Ok(updated);
    }
}
