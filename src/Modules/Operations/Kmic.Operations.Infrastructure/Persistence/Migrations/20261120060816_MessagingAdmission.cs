using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MessagingAdmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 容量保护的状态表属于本模块，建表语句由框架提供
            migrationBuilder.Sql(AdmissionTable.CreateSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {AdmissionTable.Name};");
        }
    }
}
