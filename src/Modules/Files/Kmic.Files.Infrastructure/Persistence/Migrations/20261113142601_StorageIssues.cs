using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StorageIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageIssue",
                schema: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false, comment: "出问题的文件"),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "种类"),
                    ObjectVersionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "发现时文件记录的对象版本"),
                    FoundAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "发现的时间"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "标为已解决的时间"),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "标为已解决的人"),
                    ResolutionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "处理说明，如还原的工单号"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageIssue", x => x.Id);
                    table.CheckConstraint("CK_StorageIssue_Kind", "\"Kind\" IN ('RestorableMissing', 'LostMissing')");
                    table.CheckConstraint("CK_StorageIssue_Status", "\"Status\" IN ('Open', 'Resolved')");
                },
                comment: "存储问题；Files 模块；对账发现库里有记录而存储里找不到对象的文件，待运维还原后标为已解决");

            migrationBuilder.CreateIndex(
                name: "IX_StorageIssue_CreatedAt",
                schema: "Files",
                table: "StorageIssue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StorageIssue_FileId",
                schema: "Files",
                table: "StorageIssue",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageIssue_Status_FoundAt",
                schema: "Files",
                table: "StorageIssue",
                columns: new[] { "Status", "FoundAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageIssue",
                schema: "Files");
        }
    }
}
