using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbandonReason",
                schema: "Notifications",
                table: "Delivery",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "放弃处理的原因");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AbandonedAt",
                schema: "Notifications",
                table: "Delivery",
                type: "timestamp with time zone",
                nullable: true,
                comment: "放弃处理的时间，没有放弃为空");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbandonReason",
                schema: "Notifications",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "AbandonedAt",
                schema: "Notifications",
                table: "Delivery");
        }
    }
}
