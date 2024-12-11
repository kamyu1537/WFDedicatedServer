using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WFDS.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banned_players",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    steam_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    banned_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banned_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    steam_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    last_joined_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_banned_players_steam_id",
                table: "banned_players",
                column: "steam_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_steam_id",
                table: "players",
                column: "steam_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banned_players");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
