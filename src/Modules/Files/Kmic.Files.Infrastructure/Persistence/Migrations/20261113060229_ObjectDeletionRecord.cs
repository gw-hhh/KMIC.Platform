using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ObjectDeletionRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ObjectDeletedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "对象在存储里被删除的时间：只有彻底删除的文件需要，空表示对象还在、等删除工作器处理");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectDeletedAt",
                schema: "Files",
                table: "StoredFile");
        }
    }
}
