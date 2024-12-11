using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WFDS.Database.DbSet;

[Index(nameof(SteamId), IsUnique = true)]
public class BannedPlayer
{
    [Key]
    [Column("id")]
    public long Id { get; init; }

    [Column("steam_id")]
    public ulong SteamId { get; init; }

    [MaxLength(32)]
    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("banned_at")]
    public DateTimeOffset BannedAt { get; set; } = DateTimeOffset.UtcNow;
}