using Xunit;

namespace Spike06.Tests;

// 原型 #10：两个测试类并行、各用自己的克隆库，互不可见；夹具在整个程序集只创建一次
public sealed class IsolationTestsA(ClassDatabase db) : IClassFixture<ClassDatabase>
{
    [Fact]
    public Task Class_sees_only_its_own_rows() => IsolationCheck.RunAsync(db, "A");
}
