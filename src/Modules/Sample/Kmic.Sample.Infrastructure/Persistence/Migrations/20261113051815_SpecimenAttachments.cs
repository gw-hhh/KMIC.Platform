using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SpecimenAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecimenAttachment",
                schema: "Sample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false, comment: "文件，Files 模块里的 ID"),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "关联时的文件名快照"),
                    Size = table.Column<long>(type: "bigint", nullable: false, comment: "关联时的字节数快照"),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "关联时的文件类型编码快照"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Specimen ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecimenAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecimenAttachment_Specimen_SpecimenId",
                        column: x => x.SpecimenId,
                        principalSchema: "Sample",
                        principalTable: "Specimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "示例标本的附件；Sample 模块；一行代表一个附件，保存文件 ID 和关联时的快照，随标本一起保存");

            migrationBuilder.CreateIndex(
                name: "IX_SpecimenAttachment_SpecimenId_FileId",
                schema: "Sample",
                table: "SpecimenAttachment",
                columns: new[] { "SpecimenId", "FileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecimenAttachment",
                schema: "Sample");
        }
    }
}
