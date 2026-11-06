using Shouldly;

namespace Spike06.Tests;

internal static class IsolationCheck
{
    public static async Task RunAsync(ClassDatabase db, string marker)
    {
        await Db.ExecAsync(db.Source, """INSERT INTO app."Sample" ("Id", "Name") VALUES (@id, @name)""", ("id", Guid.CreateVersion7()), ("name", marker));

        // 让两个类的测试在时间上重叠
        await Db.ExecAsync(db.Source, "SELECT pg_sleep(1)");

        (await Db.ScalarAsync<long>(db.Source, """SELECT count(*) FROM app."Sample" """)).ShouldBe(1);
        TemplateDatabase.Instances.ShouldBe(1);
    }
}
