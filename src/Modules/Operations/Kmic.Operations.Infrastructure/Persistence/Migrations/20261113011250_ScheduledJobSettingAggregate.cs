using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kmic.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScheduledJobSettingAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 表在初始迁移里由框架的建表语句建好，主键是任务名；这里改成聚合的形状：Id 做主键，任务名唯一，补上创建与并发版本列。
            // 框架读设置时只用 JobName、Enabled、Cron 三列，不受影响。表里应该还没有行（此前没有写它的地方），
            // 有的话保留并补上缺的列
            migrationBuilder.Sql("""
                ALTER TABLE "Operations"."ScheduledJobSetting" DROP CONSTRAINT "PK_ScheduledJobSetting";
                ALTER TABLE "Operations"."ScheduledJobSetting"
                    ADD COLUMN "Id" uuid,
                    ADD COLUMN "CreatedAt" timestamp with time zone,
                    ADD COLUMN "CreatedBy" uuid;
                UPDATE "Operations"."ScheduledJobSetting"
                    SET "Id" = gen_random_uuid(), "CreatedAt" = "UpdatedAt", "CreatedBy" = COALESCE("UpdatedBy", '00000000-0000-0000-0000-000000000001');
                ALTER TABLE "Operations"."ScheduledJobSetting"
                    ALTER COLUMN "Id" SET NOT NULL,
                    ALTER COLUMN "CreatedAt" SET NOT NULL,
                    ALTER COLUMN "CreatedBy" SET NOT NULL,
                    ALTER COLUMN "UpdatedAt" DROP NOT NULL,
                    ALTER COLUMN "UpdatedAt" DROP DEFAULT,
                    ALTER COLUMN "ConcurrencyVersion" TYPE bigint,
                    ALTER COLUMN "ConcurrencyVersion" DROP DEFAULT,
                    ADD CONSTRAINT "PK_ScheduledJobSetting" PRIMARY KEY ("Id");
                CREATE UNIQUE INDEX "IX_ScheduledJobSetting_JobName" ON "Operations"."ScheduledJobSetting" ("JobName");
                CREATE INDEX "IX_ScheduledJobSetting_CreatedAt" ON "Operations"."ScheduledJobSetting" ("CreatedAt");
                COMMENT ON TABLE "Operations"."ScheduledJobSetting" IS '定时任务的覆盖值；Operations 模块；运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值；框架同步触发器时读这张表';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."Id" IS 'ID';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."JobName" IS '任务的稳定名称，如 Identity.PurgeSessions';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."Enabled" IS '是否启用；空表示跟着代码里的默认值（启用）';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."Cron" IS '执行时间的 Cron 表达式；空表示跟着代码里的默认值';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."CreatedAt" IS '创建时间';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."CreatedBy" IS '创建人';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."UpdatedAt" IS '修改时间';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."UpdatedBy" IS '修改人';
                COMMENT ON COLUMN "Operations"."ScheduledJobSetting"."ConcurrencyVersion" IS '并发版本号';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql("""
                DROP INDEX "Operations"."IX_ScheduledJobSetting_CreatedAt";
                DROP INDEX "Operations"."IX_ScheduledJobSetting_JobName";
                ALTER TABLE "Operations"."ScheduledJobSetting" DROP CONSTRAINT "PK_ScheduledJobSetting";
                UPDATE "Operations"."ScheduledJobSetting" SET "UpdatedAt" = COALESCE("UpdatedAt", "CreatedAt");
                ALTER TABLE "Operations"."ScheduledJobSetting"
                    DROP COLUMN "Id",
                    DROP COLUMN "CreatedAt",
                    DROP COLUMN "CreatedBy",
                    ALTER COLUMN "UpdatedAt" SET NOT NULL,
                    ALTER COLUMN "UpdatedAt" SET DEFAULT clock_timestamp(),
                    ALTER COLUMN "ConcurrencyVersion" TYPE integer,
                    ALTER COLUMN "ConcurrencyVersion" SET DEFAULT 0,
                    ADD CONSTRAINT "PK_ScheduledJobSetting" PRIMARY KEY ("JobName");
                COMMENT ON TABLE "Operations"."ScheduledJobSetting" IS '定时任务的覆盖值：只存改过的项，没改的按代码里的默认值';
                """);
    }
}
