using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyModelComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ActualUserId",
                schema: "Operations",
                table: "AuditLog",
                type: "uuid",
                nullable: true,
                comment: "模拟身份时的实际操作人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "模拟登录时真正动手的人");

            migrationBuilder.AlterTable(
                name: "ScheduledJobSetting",
                schema: "Operations",
                comment: "定时任务的自定义设置；Operations 模块；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器",
                oldComment: "定时任务的覆盖值；Operations 模块；运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值；框架同步触发器时读这张表");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "ScheduledJobSetting",
                schema: "Operations",
                comment: "定时任务的覆盖值；Operations 模块；运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值；框架同步触发器时读这张表",
                oldComment: "定时任务的自定义设置；Operations 模块；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActualUserId",
                schema: "Operations",
                table: "AuditLog",
                type: "uuid",
                nullable: true,
                comment: "模拟登录时真正动手的人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "模拟身份时的实际操作人");
        }
    }
}
