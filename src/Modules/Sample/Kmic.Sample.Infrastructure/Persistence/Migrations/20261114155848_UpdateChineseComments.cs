using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChineseComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SpecimenItem",
                schema: "Sample",
                comment: "示例明细（Sample）：一条记录对应一项明细，随示例一起保存",
                oldComment: "示例标本的明细；Sample 模块；一行代表一项明细，随标本一起保存");

            migrationBuilder.AlterTable(
                name: "SpecimenAttachment",
                schema: "Sample",
                comment: "示例附件（Sample）：一条记录对应一个附件，保存文件 ID 及关联时的文件信息，随示例一起保存",
                oldComment: "示例标本的附件；Sample 模块；一行代表一个附件，保存文件 ID 和关联时的快照，随标本一起保存");

            migrationBuilder.AlterTable(
                name: "Specimen",
                schema: "Sample",
                comment: "业务模块示例（Sample）：一条记录对应一个示例；草稿可软删除，提交后可关闭",
                oldComment: "示例标本；Sample 模块；一行代表一个标本；可软删除，有状态流转");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                schema: "Sample",
                table: "Specimen",
                type: "uuid",
                nullable: false,
                comment: "负责人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "负责人，创建者");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgUnitId",
                schema: "Sample",
                table: "Specimen",
                type: "uuid",
                nullable: false,
                comment: "所属组织",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "所属组织，数据范围按它判断");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SpecimenItem",
                schema: "Sample",
                comment: "示例标本的明细；Sample 模块；一行代表一项明细，随标本一起保存",
                oldComment: "示例明细（Sample）：一条记录对应一项明细，随示例一起保存");

            migrationBuilder.AlterTable(
                name: "SpecimenAttachment",
                schema: "Sample",
                comment: "示例标本的附件；Sample 模块；一行代表一个附件，保存文件 ID 和关联时的快照，随标本一起保存",
                oldComment: "示例附件（Sample）：一条记录对应一个附件，保存文件 ID 及关联时的文件信息，随示例一起保存");

            migrationBuilder.AlterTable(
                name: "Specimen",
                schema: "Sample",
                comment: "示例标本；Sample 模块；一行代表一个标本；可软删除，有状态流转",
                oldComment: "业务模块示例（Sample）：一条记录对应一个示例；草稿可软删除，提交后可关闭");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                schema: "Sample",
                table: "Specimen",
                type: "uuid",
                nullable: false,
                comment: "负责人，创建者",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "负责人");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgUnitId",
                schema: "Sample",
                table: "Specimen",
                type: "uuid",
                nullable: false,
                comment: "所属组织，数据范围按它判断",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "所属组织");
        }
    }
}
