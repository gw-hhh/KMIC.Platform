using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class NumberCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NumberCounter",
                schema: "Probe",
                columns: table => new
                {
                    Rule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodKey = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PartitionKey = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastValue = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberCounter", x => new { x.Rule, x.PeriodKey, x.PartitionKey });
                },
                comment: "业务编号的计数");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumberCounter",
                schema: "Probe");
        }
    }
}
