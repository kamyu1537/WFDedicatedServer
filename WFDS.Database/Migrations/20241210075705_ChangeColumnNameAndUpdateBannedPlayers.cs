using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WFDS.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnNameAndUpdateBannedPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "players",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "players",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SteamId",
                table: "players",
                newName: "steam_id");

            migrationBuilder.RenameColumn(
                name: "LastJoinedAt",
                table: "players",
                newName: "last_joined_at");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "players",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "players",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_players_SteamId",
                table: "players",
                newName: "IX_players_steam_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "banned_players",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SteamId",
                table: "banned_players",
                newName: "steam_id");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "banned_players",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "banned_players",
                newName: "banned_at");

            migrationBuilder.RenameIndex(
                name: "IX_banned_players_SteamId",
                table: "banned_players",
                newName: "IX_banned_players_steam_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "players",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "players",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "steam_id",
                table: "players",
                newName: "SteamId");

            migrationBuilder.RenameColumn(
                name: "last_joined_at",
                table: "players",
                newName: "LastJoinedAt");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "players",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "players",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_players_steam_id",
                table: "players",
                newName: "IX_players_SteamId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "banned_players",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "steam_id",
                table: "banned_players",
                newName: "SteamId");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "banned_players",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "banned_at",
                table: "banned_players",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_banned_players_steam_id",
                table: "banned_players",
                newName: "IX_banned_players_SteamId");
        }
    }
}
