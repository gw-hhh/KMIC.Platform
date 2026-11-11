using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Echo.Probe.Migrations
{
    /// <inheritdoc />
    public partial class ReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketSummary",
                schema: "Echo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceVersion = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    TerminalReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TerminalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RebuildRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSummary", x => x.Id);
                },
                comment: "Probe 单据的读模型");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketSummary",
                schema: "Echo");
        }
    }
}
