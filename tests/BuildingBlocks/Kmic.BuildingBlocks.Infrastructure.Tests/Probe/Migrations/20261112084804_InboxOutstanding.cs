using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class InboxOutstanding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Probe",
                table: "InboxMessage",
                column: "Subscription",
                filter: "\"Status\" IN (0, 1, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxMessage_Subscription",
                schema: "Probe",
                table: "InboxMessage");
        }
    }
}
