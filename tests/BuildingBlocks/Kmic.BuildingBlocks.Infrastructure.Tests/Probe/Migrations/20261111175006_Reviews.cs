using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class Reviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Review",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "标题"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    Revision = table.Column<int>(type: "integer", nullable: false, comment: "业务版本"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.Id);
                    table.CheckConstraint("CK_Review_Status", "\"Status\" IN ('Draft', 'Submitted', 'Approved')");
                },
                comment: "测试评审；Probe 模块；一行代表一次评审；有状态流转");

            migrationBuilder.CreateTable(
                name: "ReviewStateHistory",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnBehalfOfUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewStateHistory", x => x.Id);
                },
                comment: "Review 的状态流转记录，只追加");

            migrationBuilder.CreateIndex(
                name: "IX_Review_CreatedAt",
                schema: "Probe",
                table: "Review",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStateHistory_AggregateId_OccurredAt",
                schema: "Probe",
                table: "ReviewStateHistory",
                columns: new[] { "AggregateId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Review",
                schema: "Probe");

            migrationBuilder.DropTable(
                name: "ReviewStateHistory",
                schema: "Probe");
        }
    }
}
