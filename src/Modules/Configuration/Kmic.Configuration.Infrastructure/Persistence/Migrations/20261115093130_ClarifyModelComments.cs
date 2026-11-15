using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyModelComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SystemSettingValue",
                schema: "Configuration",
                comment: "系统设置的已保存值；Configuration 模块；每个设置一行，无记录时使用代码默认值",
                oldComment: "被修改过的系统设置值；Configuration 模块；一个设置一行，没有这一行就用代码里的默认值");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SystemSettingValue",
                schema: "Configuration",
                comment: "被修改过的系统设置值；Configuration 模块；一个设置一行，没有这一行就用代码里的默认值",
                oldComment: "系统设置的已保存值；Configuration 模块；每个设置一行，无记录时使用代码默认值");
        }
    }
}
