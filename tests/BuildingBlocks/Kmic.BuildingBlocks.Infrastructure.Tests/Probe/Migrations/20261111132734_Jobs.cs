using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class Jobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackgroundJob",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Args = table.Column<string>(type: "jsonb", nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    ProgressDone = table.Column<long>(type: "bigint", nullable: false),
                    ProgressTotal = table.Column<long>(type: "bigint", nullable: true),
                    ProgressMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Checkpoint = table.Column<string>(type: "jsonb", nullable: true),
                    ResultFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultSummary = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelRequestedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJob", x => x.Id);
                },
                comment: "后台作业");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_LeaseUntil",
                schema: "Probe",
                table: "BackgroundJob",
                column: "LeaseUntil",
                filter: "\"Status\" IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJob_SubmittedBy_SubmittedAt",
                schema: "Probe",
                table: "BackgroundJob",
                columns: new[] { "SubmittedBy", "SubmittedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJob",
                schema: "Probe");
        }
    }
}
