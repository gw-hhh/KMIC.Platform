using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChineseComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "StoredFile",
                schema: "Files",
                comment: "文件元数据（Files）：一条记录对应一个文件，内容存于对象存储；到期清理先删对象再删记录，彻底删除保留记录",
                oldComment: "文件元数据；Files 模块；一行一个文件，内容在对象存储里；状态是删除队列：删除中的行由删除工作器领取，先删对象再删行");

            migrationBuilder.AlterTable(
                name: "StorageIssue",
                schema: "Files",
                comment: "存储问题（Files）：一条记录对应一次对象缺失问题；登记处理结果，不执行还原，文件记录清理后仍保留",
                oldComment: "存储问题；Files 模块；对账发现库里有记录而存储里找不到对象的文件，待运维还原后标为已解决");

            migrationBuilder.AlterColumn<Guid>(
                name: "UploadedBy",
                schema: "Files",
                table: "StoredFile",
                type: "uuid",
                nullable: true,
                comment: "上传人或生成请求人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "上传的人；系统生成的文件没有");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UploadedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: false,
                comment: "文件登记时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "上传开始的时间");

            migrationBuilder.AlterColumn<string>(
                name: "RejectReason",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "拒绝原因",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "被拒绝的原因：类型不符、超过大小、扫描没通过、写入失败");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurgedBy",
                schema: "Files",
                table: "StoredFile",
                type: "uuid",
                nullable: true,
                comment: "彻底删除操作人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "彻底删除的人");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PurgedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "彻底删除标记时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "彻底删除的时间");

            migrationBuilder.AlterColumn<string>(
                name: "Profile",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "上传配置名称",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "上传档案名；生成的文件是 Generated:来源");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalName",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "显示文件名",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "显示用的文件名：去掉路径与控制字符，只作显示");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ObjectDeletedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "对象删除登记时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "对象在存储里被删除的时间：只有彻底删除的文件需要，空表示对象还在、等删除工作器处理");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "HistoryPurgedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "历史版本清除登记时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "运维彻底删除历史版本之后登记的时间；为空表示历史版本可能仍在");

            migrationBuilder.AlterColumn<string>(
                name: "HistoryPurgeNote",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "历史版本清除记录",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "彻底删除历史版本的执行记录，如工单号");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "清理到期时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "成为清理候选的时间：临时、已解除关联、被拒绝的文件才有，已关联的文件为空");

            migrationBuilder.AlterColumn<int>(
                name: "DeleteAttempts",
                schema: "Files",
                table: "StoredFile",
                type: "integer",
                nullable: false,
                comment: "删除队列领取次数，人工重试时清零",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "删除队列已经试了几次");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "StoredFile",
                schema: "Files",
                comment: "文件元数据；Files 模块；一行一个文件，内容在对象存储里；状态是删除队列：删除中的行由删除工作器领取，先删对象再删行",
                oldComment: "文件元数据（Files）：一条记录对应一个文件，内容存于对象存储；到期清理先删对象再删记录，彻底删除保留记录");

            migrationBuilder.AlterTable(
                name: "StorageIssue",
                schema: "Files",
                comment: "存储问题；Files 模块；对账发现库里有记录而存储里找不到对象的文件，待运维还原后标为已解决",
                oldComment: "存储问题（Files）：一条记录对应一次对象缺失问题；登记处理结果，不执行还原，文件记录清理后仍保留");

            migrationBuilder.AlterColumn<Guid>(
                name: "UploadedBy",
                schema: "Files",
                table: "StoredFile",
                type: "uuid",
                nullable: true,
                comment: "上传的人；系统生成的文件没有",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "上传人或生成请求人");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UploadedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: false,
                comment: "上传开始的时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "文件登记时间");

            migrationBuilder.AlterColumn<string>(
                name: "RejectReason",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "被拒绝的原因：类型不符、超过大小、扫描没通过、写入失败",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "拒绝原因");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurgedBy",
                schema: "Files",
                table: "StoredFile",
                type: "uuid",
                nullable: true,
                comment: "彻底删除的人",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "彻底删除操作人");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PurgedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "彻底删除的时间",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "彻底删除标记时间");

            migrationBuilder.AlterColumn<string>(
                name: "Profile",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "上传档案名；生成的文件是 Generated:来源",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "上传配置名称");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalName",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "显示用的文件名：去掉路径与控制字符，只作显示",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "显示文件名");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ObjectDeletedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "对象在存储里被删除的时间：只有彻底删除的文件需要，空表示对象还在、等删除工作器处理",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "对象删除登记时间");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "HistoryPurgedAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "运维彻底删除历史版本之后登记的时间；为空表示历史版本可能仍在",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "历史版本清除登记时间");

            migrationBuilder.AlterColumn<string>(
                name: "HistoryPurgeNote",
                schema: "Files",
                table: "StoredFile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "彻底删除历史版本的执行记录，如工单号",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "历史版本清除记录");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpiresAt",
                schema: "Files",
                table: "StoredFile",
                type: "timestamp with time zone",
                nullable: true,
                comment: "成为清理候选的时间：临时、已解除关联、被拒绝的文件才有，已关联的文件为空",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "清理到期时间");

            migrationBuilder.AlterColumn<int>(
                name: "DeleteAttempts",
                schema: "Files",
                table: "StoredFile",
                type: "integer",
                nullable: false,
                comment: "删除队列已经试了几次",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "删除队列领取次数，人工重试时清零");
        }
    }
}
