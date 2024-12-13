using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFDS.Database.DbSet;

public class PlayerLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("player_id")]
    public ulong PlayerId { get; set; }
    
    [Column("display_name")]
    [MaxLength(32)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Column("zone")]
    [MaxLength(128)]
    public string Zone { get; set; } = string.Empty;
    
    [Column("zone_owner")]
    public long ZoneOwner { get; set; }
    
    [Column("position_x")]
    public float PositionX { get; set; }
    
    [Column("position_y")]
    public float PositionY { get; set; }
    
    [Column("position_z")]
    public float PositionZ { get; set; }
    
    [Column("action")]
    [MaxLength(128)]
    public string Action { get; set; } = string.Empty;
    
    [Column("message", TypeName = "TEXT")]
    public string Message { get; set; } = string.Empty;
    
    [Column("json_data", TypeName = "TEXT")]
    public string JsonData { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}