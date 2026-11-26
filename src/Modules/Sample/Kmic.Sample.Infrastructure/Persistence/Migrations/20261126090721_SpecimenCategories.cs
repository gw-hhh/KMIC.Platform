using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SpecimenCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecimenCategory",
                schema: "Sample",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "名称，唯一", collation: "CaseInsensitive"),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecimenCategory", x => x.Id);
                },
                comment: "示例分类（Sample）：简单维护档的示例，一行一个分类");

            migrationBuilder.CreateIndex(
                name: "IX_SpecimenCategory_CreatedAt",
                schema: "Sample",
                table: "SpecimenCategory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SpecimenCategory_Name",
                schema: "Sample",
                table: "SpecimenCategory",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecimenCategory",
                schema: "Sample");
        }
    }
}
