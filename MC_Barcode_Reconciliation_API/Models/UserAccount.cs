using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MC_Barcode_Reconciliation_API.Models;

[Table("User_Account")]
public class UserAccount
{
    [Key]
    [Column("USERID")]
    [MaxLength(50)]
    public string? UserId { get; set; }

    [Column("PASSWORD")]
    [MaxLength(50)]
    public string? Password { get; set; }

    [Column("LEVEL")]
    [MaxLength(10)]
    public string? Level { get; set; }

    [Column("SITE")]
    [MaxLength(3)]
    public string? Site { get; set; }

    [Column("SystemName")]
    [MaxLength(900)]
    public string? SystemName { get; set; }

    [Column("DEFAULT_LOCATION")]
    [MaxLength(50)]
    public string? DefaultLocation { get; set; }

    [Column("PLANT_LOC")]
    [MaxLength(50)]
    public string? PlantLoc { get; set; }

    [Column("IFS_SITE")]
    [MaxLength(50)]
    public string? IfsSite { get; set; }

    [Column("IFS_SITE_NAME")]
    [MaxLength(50)]
    public string? IfsSiteName { get; set; }

    [Column("RecevingCat")]
    [MaxLength(50)]
    public string? RecevingCat { get; set; }
}
