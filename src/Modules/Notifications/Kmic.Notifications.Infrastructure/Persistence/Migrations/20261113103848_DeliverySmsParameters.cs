using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeliverySmsParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmsParameters",
                schema: "Notifications",
                table: "Delivery",
                type: "jsonb",
                nullable: true,
                comment: "短信模板的参数，JSON；只有短信有。短信服务只收模板编码和参数，实际发送用它，含密模板投递完成后清空");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Status_Priority",
                schema: "Notifications",
                table: "Delivery",
                columns: new[] { "Status", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Delivery_Status_Priority",
                schema: "Notifications",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "SmsParameters",
                schema: "Notifications",
                table: "Delivery");
        }
    }
}
