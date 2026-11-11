using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Echo.Probe.Migrations
{
    /// <inheritdoc />
    public partial class Purge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Echo",
                table: "InboxMessage",
                column: "ReceivedAt",
                filter: "\"Status\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Echo",
                table: "BackgroundJob",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxMessage_ReceivedAt",
                schema: "Echo",
                table: "InboxMessage");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundJob_ExpiresAt",
                schema: "Echo",
                table: "BackgroundJob");
        }
    }
}
