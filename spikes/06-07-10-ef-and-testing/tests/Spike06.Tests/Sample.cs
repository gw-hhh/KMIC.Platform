namespace Spike06.Tests;

// 新版代码中的实体：属性已改名为 DisplayName
public sealed class Sample
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = null!;
}
