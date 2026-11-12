using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuthenticationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginThrottle",
                schema: "Identity",
                columns: table => new
                {
                    LoginNameHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    LockLevel = table.Column<int>(type: "integer", nullable: false),
                    NextAllowedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginThrottle", x => x.LoginNameHash);
                },
                comment: "登录失败计数；Identity 模块；一行代表一个登录名（哈希）的失败状态；24 小时没有新失败由每日清理任务删除");

            migrationBuilder.CreateTable(
                name: "VerificationCode",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AttemptsLeft = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequestIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCode", x => x.Id);
                },
                comment: "短信验证码；Identity 模块；一行代表一次发送请求（哈希），过期超过 1 天由每日清理任务删除");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCode_PhoneHash_CreatedAt",
                schema: "Identity",
                table: "VerificationCode",
                columns: new[] { "PhoneHash", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCode_RequestIp_CreatedAt",
                schema: "Identity",
                table: "VerificationCode",
                columns: new[] { "RequestIp", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginThrottle",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "VerificationCode",
                schema: "Identity");
        }
    }
}
