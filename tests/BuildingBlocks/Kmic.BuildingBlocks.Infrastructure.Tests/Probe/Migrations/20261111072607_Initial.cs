using System;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InitializeModuleSchema("Probe");

            migrationBuilder.EnsureSchema(
                name: "Probe");

            migrationBuilder.CreateTable(
                name: "IdempotencyRecord",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseBody = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRecord", x => x.Id);
                },
                comment: "幂等记录");

            migrationBuilder.CreateTable(
                name: "Ticket",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "单据编号", collation: "CaseInsensitive"),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "标题"),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属人"),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属组织"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "可用额度"),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "截止时间"),
                    Note = table.Column<string>(type: "text", nullable: true, comment: "备注"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    Priority = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "优先级"),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "联系邮箱"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "删除时间"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "删除人"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否已删除"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    BudgetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "预算：数额"),
                    BudgetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, comment: "预算：币种"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号"),
                    Watchers = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.CheckConstraint("CK_Ticket_Priority", "\"Priority\" IN ('Normal', 'Urgent')");
                    table.CheckConstraint("CK_Ticket_Status", "\"Status\" IN ('Open', 'Closed')");
                },
                comment: "测试单据；Probe 模块；一行代表一张单据；可软删除");

            migrationBuilder.CreateTable(
                name: "TicketLine",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Text = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "内容"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Ticket ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLine_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "Probe",
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "测试单据的明细；Probe 模块；一行代表一条明细，随单据保存");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_ExpiresAt",
                schema: "Probe",
                table: "IdempotencyRecord",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRecord_UserId_RoleId_Operation_Key",
                schema: "Probe",
                table: "IdempotencyRecord",
                columns: new[] { "UserId", "RoleId", "Operation", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CreatedAt",
                schema: "Probe",
                table: "Ticket",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Number",
                schema: "Probe",
                table: "Ticket",
                column: "Number",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLine_TicketId",
                schema: "Probe",
                table: "TicketLine",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotencyRecord",
                schema: "Probe");

            migrationBuilder.DropTable(
                name: "TicketLine",
                schema: "Probe");

            migrationBuilder.DropTable(
                name: "Ticket",
                schema: "Probe");
        }
    }
}
