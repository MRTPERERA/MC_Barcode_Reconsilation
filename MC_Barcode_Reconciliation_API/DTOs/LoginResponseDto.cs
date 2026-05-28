namespace MC_Barcode_Reconciliation_API.DTOs;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserInfoDto? User { get; set; }
}

public class UserInfoDto
{
    public string? UserId { get; set; }
    public string? Level { get; set; }
    public string? Site { get; set; }
    public string? SystemName { get; set; }
    public string? DefaultLocation { get; set; }
    public string? PlantLoc { get; set; }
    public string? IfsSite { get; set; }
    public string? IfsSiteName { get; set; }
    public string? RecevingCat { get; set; }
}
