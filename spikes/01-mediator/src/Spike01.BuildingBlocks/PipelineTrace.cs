namespace Spike01.BuildingBlocks;

/// <summary>
/// 原型用：记录每个作用域内管道行为与处理器的执行顺序
/// </summary>
public sealed class PipelineTrace
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string entry) => _entries.Add(entry);
}
