using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WFDS.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddChatHistoryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_chat_histories_created_at",
                table: "chat_histories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_chat_histories_zone_zone_owner",
                table: "chat_histories",
                columns: new[] { "zone", "zone_owner" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_chat_histories_created_at",
                table: "chat_histories");

            migrationBuilder.DropIndex(
                name: "IX_chat_histories_zone_zone_owner",
                table: "chat_histories");
        }
    }
}
