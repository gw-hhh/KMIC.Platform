using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Spike06.Tests;

// 原型 #7：.MirrorTo(...) 过渡映射；以及 06 第 11.3 节列改名示例中发布 1 读哪一列
public sealed class MirrorTests(ClassDatabase db) : IClassFixture<ClassDatabase>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Mirror_writes_both_columns_on_insert_and_update()
    {
        var id = Guid.CreateVersion7();
        await using (var context = new Release2Context(db.ConnectionString))
        {
            context.Samples.Add(new Sample { Id = id, DisplayName = "甲" });
            await context.SaveChangesAsync(Ct);
        }

        (await ColumnsAsync(id)).ShouldBe(("甲", "甲"));

        await using (var context = new Release2Context(db.ConnectionString))
        {
            (await context.Samples.SingleAsync(s => s.Id == id, Ct)).DisplayName = "乙";
            await context.SaveChangesAsync(Ct);
        }

        (await ColumnsAsync(id)).ShouldBe(("乙", "乙"));
    }

    [Fact]
    public async Task Original_release_1_reads_stale_values_written_by_the_previous_version()
    {
        var (existing, createdByOldVersion) = await RollingWindowAsync();

        // 原写法：发布 1 读新列；滚动期间上一版只写旧列
        await using var context = new DocRelease1Context(db.ConnectionString);
        (await context.Samples.SingleAsync(s => s.Id == existing, Ct)).DisplayName.ShouldBe("原名");

        // 上一版新建的行新列为空：属性不可空，读取直接抛异常，列表与详情返回 500
        await Should.ThrowAsync<InvalidCastException>(() => context.Samples.SingleAsync(s => s.Id == createdByOldVersion, Ct));
    }

    [Fact]
    public async Task Revised_release_1_reads_the_old_column_until_the_previous_version_is_gone()
    {
        var (existing, createdByOldVersion) = await RollingWindowAsync();

        // 修订后：发布 1 仍读旧列，同时写两列
        await using (var context = new FixedRelease1Context(db.ConnectionString))
        {
            (await context.Samples.SingleAsync(s => s.Id == existing, Ct)).DisplayName.ShouldBe("上一版改名");
            (await context.Samples.SingleAsync(s => s.Id == createdByOldVersion, Ct)).DisplayName.ShouldBe("上一版新建");
            (await context.Samples.SingleAsync(s => s.Id == existing, Ct)).DisplayName = "本版改名";
            await context.SaveChangesAsync(Ct);
        }

        (await ColumnsAsync(existing)).ShouldBe(("本版改名", "本版改名"));
        await using (var legacy = new LegacyContext(db.ConnectionString))
        {
            (await legacy.Samples.SingleAsync(s => s.Id == existing, Ct)).Name.ShouldBe("本版改名");
        }

        // 发布 2：上一版已全部退出，再次回填后改为读新列
        await BackfillAsync();
        await using var release2 = new Release2Context(db.ConnectionString);
        (await release2.Samples.SingleAsync(s => s.Id == createdByOldVersion, Ct)).DisplayName.ShouldBe("上一版新建");
    }

    [Fact]
    public async Task Bulk_update_bypasses_the_mirror()
    {
        var id = Guid.CreateVersion7();
        await using var context = new Release2Context(db.ConnectionString);
        context.Samples.Add(new Sample { Id = id, DisplayName = "甲" });
        await context.SaveChangesAsync(Ct);

        await context.Samples.Where(s => s.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.DisplayName, "批量"), Ct);

        // ExecuteUpdate 不经过保存拦截器，旧列没有同步
        (await ColumnsAsync(id)).ShouldBe(("批量", "甲"));
    }

    // 发布前已有一行；发布 1 的迁移回填；滚动期间上一版改了这一行、又新建了一行
    private async Task<(Guid Existing, Guid CreatedByOldVersion)> RollingWindowAsync()
    {
        var existing = Guid.CreateVersion7();
        var created = Guid.CreateVersion7();
        await using (var legacy = new LegacyContext(db.ConnectionString))
        {
            legacy.Samples.Add(new LegacySample { Id = existing, Name = "原名" });
            await legacy.SaveChangesAsync(Ct);
        }

        await BackfillAsync();

        await using (var legacy = new LegacyContext(db.ConnectionString))
        {
            (await legacy.Samples.SingleAsync(s => s.Id == existing, Ct)).Name = "上一版改名";
            legacy.Samples.Add(new LegacySample { Id = created, Name = "上一版新建" });
            await legacy.SaveChangesAsync(Ct);
        }

        return (existing, created);
    }

    private Task<int> BackfillAsync() =>
        Db.ExecAsync(db.Source, """UPDATE app."Sample" SET "DisplayName" = "Name" WHERE "DisplayName" IS DISTINCT FROM "Name" """);

    private async Task<(string? DisplayName, string? Name)> ColumnsAsync(Guid id)
    {
        var row = (await Db.QueryAsync(db.Source, """SELECT "DisplayName", "Name" FROM app."Sample" WHERE "Id" = @id""", ("id", id)))[0];
        return ((string?)row[0], (string?)row[1]);
    }
}
