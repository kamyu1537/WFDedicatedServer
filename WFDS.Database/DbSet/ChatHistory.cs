using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WFDS.Database.DbSet;

[Index(nameof(PlayerId))]
public class ChatHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("player_id")]
    public ulong PlayerId { get; set; }
    
    [Column("display_name")]
    [MaxLength(32)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Column("message", TypeName = "TEXT")]
    public string Message { get; set; } = string.Empty;
    
    [Column("zone")]
    [MaxLength(128)]
    public string Zone { get; set; } = string.Empty;
    
    [Column("zone_owner")]
    public long ZoneOwner { get; set; }
    
    [Column("is_local")]
    public bool IsLocal { get; set; }
    
    [Column("position_x")]
    public float PositionX { get; set; }
    
    [Column("position_y")]
    public float PositionY { get; set; }
    
    [Column("position_z")]
    public float PositionZ { get; set; }
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}