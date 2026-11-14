using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChineseComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "VerificationCode",
                schema: "Identity",
                comment: "预留的短信验证码记录（Identity）：保存验证码哈希及校验状态；发送、校验和清理流程尚未接入",
                oldComment: "短信验证码；Identity 模块；一行代表一次发送请求（哈希），过期超过 1 天由每日清理任务删除");

            migrationBuilder.AlterTable(
                name: "Session",
                schema: "Identity",
                comment: "会话（Identity）：一条记录对应一次普通或模拟登录；按绝对到期或作废时间及配置保留期清理",
                oldComment: "会话；Identity 模块；一行代表一次登录（含模拟身份）；作废或过期 30 天后由清理任务删除");

            migrationBuilder.AlterTable(
                name: "LoginThrottle",
                schema: "Identity",
                comment: "登录失败计数（Identity）：每个登录名哈希一条失败状态记录；按配置的重置间隔清理，等待或锁定未结束时保留",
                oldComment: "登录失败计数；Identity 模块；一行代表一个登录名（哈希）的失败状态；24 小时没有新失败由每日清理任务删除");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "Identity",
                table: "User",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "密码哈希",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "密码哈希；为空表示还没有设置过密码（迁移来的员工靠短信登录）");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgUnitId",
                schema: "Identity",
                table: "User",
                type: "uuid",
                nullable: true,
                comment: "所属组织单元",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "所属组织单元，数据范围的“本组织”从它算起；为空时只剩“本人”的数据");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Identity",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "手机号",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "手机号，规范化为国际格式，填写时唯一；短信登录需要它");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "英文名",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "英文名，合同、报告上要印");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "登录名",
                collation: "CaseInsensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "登录名，唯一且不区分大小写；迁移来的用户沿用旧系统的手机号",
                oldCollation: "CaseInsensitive");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RefreshExpiresAt",
                schema: "Identity",
                table: "Session",
                type: "timestamp with time zone",
                nullable: false,
                comment: "刷新令牌或模拟会话到期时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "滑动过期时间");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                schema: "Identity",
                table: "Session",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "登录时的 User-Agent",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "设备名称，App 上报或由 User-Agent 解析");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "VerificationCode",
                schema: "Identity",
                comment: "短信验证码；Identity 模块；一行代表一次发送请求（哈希），过期超过 1 天由每日清理任务删除",
                oldComment: "预留的短信验证码记录（Identity）：保存验证码哈希及校验状态；发送、校验和清理流程尚未接入");

            migrationBuilder.AlterTable(
                name: "Session",
                schema: "Identity",
                comment: "会话；Identity 模块；一行代表一次登录（含模拟身份）；作废或过期 30 天后由清理任务删除",
                oldComment: "会话（Identity）：一条记录对应一次普通或模拟登录；按绝对到期或作废时间及配置保留期清理");

            migrationBuilder.AlterTable(
                name: "LoginThrottle",
                schema: "Identity",
                comment: "登录失败计数；Identity 模块；一行代表一个登录名（哈希）的失败状态；24 小时没有新失败由每日清理任务删除",
                oldComment: "登录失败计数（Identity）：每个登录名哈希一条失败状态记录；按配置的重置间隔清理，等待或锁定未结束时保留");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "Identity",
                table: "User",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "密码哈希；为空表示还没有设置过密码（迁移来的员工靠短信登录）",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "密码哈希");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrgUnitId",
                schema: "Identity",
                table: "User",
                type: "uuid",
                nullable: true,
                comment: "所属组织单元，数据范围的“本组织”从它算起；为空时只剩“本人”的数据",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "所属组织单元");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Identity",
                table: "User",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "手机号，规范化为国际格式，填写时唯一；短信登录需要它",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "手机号");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "英文名，合同、报告上要印",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "英文名");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                schema: "Identity",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "登录名，唯一且不区分大小写；迁移来的用户沿用旧系统的手机号",
                collation: "CaseInsensitive",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "登录名",
                oldCollation: "CaseInsensitive");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RefreshExpiresAt",
                schema: "Identity",
                table: "Session",
                type: "timestamp with time zone",
                nullable: false,
                comment: "滑动过期时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "刷新令牌或模拟会话到期时间");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceName",
                schema: "Identity",
                table: "Session",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "设备名称，App 上报或由 User-Agent 解析",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "登录时的 User-Agent");
        }
    }
}
