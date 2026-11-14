using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileIds",
                schema: "Notifications",
                table: "Delivery",
                type: "jsonb",
                nullable: true,
                comment: "邮件附件的文件 ID，JSON 数组；只有邮件有，发送时才读取文件内容");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileIds",
                schema: "Notifications",
                table: "Delivery");
        }
    }
}
