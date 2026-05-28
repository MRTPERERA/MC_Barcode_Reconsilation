using MC_Barcode_Reconciliation_API.DTOs;
using MC_Barcode_Reconciliation_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MC_Barcode_Reconciliation_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate user by UserId and Password.
    /// Returns user details if login is successful.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LoginResponseDto
            {
                Success = false,
                Message = "Invalid request format."
            });

        var response = await _authService.LoginAsync(request);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    /// <summary>
    /// Get user information by UserId.
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);

        if (user is null)
            return NotFound(new { message = $"User '{userId}' not found." });

        return Ok(user);
    }

    /// <summary>
    /// Validate user credentials (returns true/false).
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "UserId and Password are required." });

        var isValid = await _authService.ValidateUserAsync(request.UserId, request.Password);

        return Ok(new ValidateResponseDto
        {
            IsValid = isValid,
            Message = isValid ? "Credentials are valid." : "Invalid credentials."
        });
    }
}

public class ValidateResponseDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
}
