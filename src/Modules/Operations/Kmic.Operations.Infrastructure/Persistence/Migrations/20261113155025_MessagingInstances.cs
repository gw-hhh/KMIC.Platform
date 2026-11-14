using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MessagingInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 实例心跳表属于本模块，建表语句由框架提供
            migrationBuilder.Sql(MessagingInstanceTable.CreateSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {MessagingInstanceTable.Name};");
        }
    }
}
