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
                comment: "幂等记录；框架表；一行代表一次带幂等键的命令；同一用户、角色、操作、键唯一；过期后由清理任务删除");

            migrationBuilder.CreateTable(
                name: "Ticket",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, collation: "CaseInsensitive"),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时刻"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人；模拟身份时为实际操作人，系统身份为系统账号"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "删除时刻"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "删除人"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否已删除（软删除）"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "最后修改时刻；新增后未修改过为空"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "最后修改人"),
                    BudgetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BudgetCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本：聚合内任何实体新增、修改、删除都加一"),
                    Watchers = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.CheckConstraint("CK_Ticket_Priority", "\"Priority\" IN ('Normal', 'Urgent')");
                    table.CheckConstraint("CK_Ticket_Status", "\"Status\" IN ('Open', 'Closed')");
                });

            migrationBuilder.CreateTable(
                name: "TicketLine",
                schema: "Probe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时刻"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人；模拟身份时为实际操作人，系统身份为系统账号"),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "最后修改时刻；新增后未修改过为空"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "最后修改人")
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
                });

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
