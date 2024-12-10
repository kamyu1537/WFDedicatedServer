using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WFDS.Database.DbSet;

[Index(nameof(SteamId), IsUnique = true)]
public class Player
{
    [Key]
    [Column("id")]
    public long Id { get; init; }
    [Column("steam_id")]
    public ulong SteamId { get; init; }

    [MaxLength(32)]
    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    [Column("last_joined_at")]
    public DateTime LastJoinedAt { get; set; } = DateTime.UtcNow;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}