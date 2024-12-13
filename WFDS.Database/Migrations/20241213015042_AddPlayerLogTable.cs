using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WFDS.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    zone = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    zone_owner = table.Column<long>(type: "INTEGER", nullable: false),
                    position_x = table.Column<float>(type: "REAL", nullable: false),
                    position_y = table.Column<float>(type: "REAL", nullable: false),
                    position_z = table.Column<float>(type: "REAL", nullable: false),
                    action = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    message = table.Column<string>(type: "TEXT", nullable: false),
                    json_data = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_logs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_logs");
        }
    }
}
