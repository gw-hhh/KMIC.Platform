using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Files.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FileAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoredFile",
                schema: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "对象在存储里的键，由文件 ID 生成，与文件名无关"),
                    OriginalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "显示用的文件名：去掉路径与控制字符，只作显示"),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "识别出的文件类型编码；写入完成之前没有"),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "识别出的内容类型，不信任客户端声明的值"),
                    Size = table.Column<long>(type: "bigint", nullable: true, comment: "实际字节数；写入完成之前没有"),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "内容的 SHA-256，小写十六进制"),
                    ObjectVersionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "存储里对象的版本 ID：开启版本控制的桶才有，恢复与对账用"),
                    Profile = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "上传档案名；生成的文件是 Generated:来源"),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "上传的人；系统生成的文件没有"),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "上传开始的时间"),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "状态"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "成为清理候选的时间：临时、已解除关联、被拒绝的文件才有，已关联的文件为空"),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "被拒绝的原因：类型不符、超过大小、扫描没通过、写入失败"),
                    PurgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "彻底删除的时间"),
                    PurgedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "彻底删除的人"),
                    PurgeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "彻底删除的原因"),
                    HistoryPurgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "运维彻底删除历史版本之后登记的时间；为空表示历史版本可能仍在"),
                    HistoryPurgeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "彻底删除历史版本的执行记录，如工单号"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    DeleteAttempts = table.Column<int>(type: "integer", nullable: false, comment: "删除队列已经试了几次"),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true, comment: "删除队列的领取令牌"),
                    LeaseUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "删除队列的领取到期时间"),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "删除队列下次可以领取的时间"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人"),
                    ConcurrencyVersion = table.Column<long>(type: "bigint", nullable: false, comment: "并发版本号")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFile", x => x.Id);
                    table.CheckConstraint("CK_StoredFile_Status", "\"Status\" IN ('Uploading', 'Scanning', 'Available', 'Attached', 'Detached', 'Rejected', 'Deleting', 'Purged')");
                },
                comment: "文件元数据；Files 模块；一行一个文件，内容在对象存储里；状态是删除队列：删除中的行由删除工作器领取，先删对象再删行");

            migrationBuilder.CreateTable(
                name: "StoredFileStateHistory",
                schema: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnBehalfOfUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFileStateHistory", x => x.Id);
                },
                comment: "StoredFile 的状态流转记录，只追加");

            migrationBuilder.CreateTable(
                name: "FileReference",
                schema: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID"),
                    OwnerModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "引用它的业务模块"),
                    OwnerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "业务对象的类型，如 Specimen"),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "业务对象的 ID"),
                    AttachedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "关联的时间"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "创建人"),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false, comment: "StoredFile ID"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "修改时间"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "修改人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileReference_StoredFile_StoredFileId",
                        column: x => x.StoredFileId,
                        principalSchema: "Files",
                        principalTable: "StoredFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "文件的引用登记；Files 模块；业务对象对文件的引用，只由关联与解除关联的请求消息维护，用来判断文件的生命周期");

            migrationBuilder.CreateIndex(
                name: "IX_FileReference_OwnerModule_OwnerType_OwnerId",
                schema: "Files",
                table: "FileReference",
                columns: new[] { "OwnerModule", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_FileReference_StoredFileId_OwnerModule_OwnerType_OwnerId",
                schema: "Files",
                table: "FileReference",
                columns: new[] { "StoredFileId", "OwnerModule", "OwnerType", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFile_CreatedAt",
                schema: "Files",
                table: "StoredFile",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFile_Profile",
                schema: "Files",
                table: "StoredFile",
                column: "Profile");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFile_Status_ExpiresAt",
                schema: "Files",
                table: "StoredFile",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFile_UploadedBy_UploadedAt",
                schema: "Files",
                table: "StoredFile",
                columns: new[] { "UploadedBy", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFileStateHistory_AggregateId_OccurredAt",
                schema: "Files",
                table: "StoredFileStateHistory",
                columns: new[] { "AggregateId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileReference",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "StoredFileStateHistory",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "StoredFile",
                schema: "Files");
        }
    }
}
