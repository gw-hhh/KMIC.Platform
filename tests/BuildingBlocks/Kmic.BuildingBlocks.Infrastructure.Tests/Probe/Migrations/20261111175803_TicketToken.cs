using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class TicketToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessTokenHash",
                schema: "Probe",
                table: "Ticket",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "访问令牌的哈希");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTokenHash",
                schema: "Probe",
                table: "Ticket");
        }
    }
}
